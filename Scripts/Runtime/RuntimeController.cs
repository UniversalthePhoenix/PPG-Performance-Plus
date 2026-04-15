using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PPGPerformancePlusMod
{
    public class RuntimeController : MonoBehaviour
    {
        private class BodyState
        {
            public float IdleTime;
            public float OffscreenTime;
            public float LastSeenTime;
            public RigidbodyInterpolation2D OriginalInterpolation = RigidbodyInterpolation2D.None;
            public bool HadOriginalInterpolation;
            public bool InterpolationReduced;
        }

        private ModConfig config;
        private FrameTracker frameTracker;
        private NotificationCenter notifications;
        private readonly Dictionary<int, BodyState> trackedBodies = new Dictionary<int, BodyState>();

        private KeyCode settingsKey = KeyCode.F10;
        private bool overlayVisible;
        private float lastNotificationTime = -999f;
        private float nextScanTime;
        private float emergencyUntilTime;
        private bool emergencyModeActive;
        private int lastRigidbodyCount;
        private int lastJointCount;
        private int consecutiveLagFrames;

        private int autoSleepActions;
        private int debrisActions;
        private int heavySpawnEvents;
        private int warningsIssued;
        private int peakRigidbodies;
        private int peakJoints;

        private SceneSnapshot latestSnapshot;

        public void NotifyCatalogRefresh()
        {
            if (config != null && config.ShowNotifications)
            {
                notifications.Push("Catalog refresh", "PPG Performance+ is active for the current scene.", 2f);
            }
        }

        public void ShutdownRuntime()
        {
            SaveAll();
        }

        private void Awake()
        {
            hideFlags = HideFlags.HideAndDontSave;
            config = ConfigStore.Load();
            frameTracker = new FrameTracker(300);
            notifications = new NotificationCenter();
            settingsKey = ParseKey(config.SettingsKey);
            overlayVisible = config.FirstRun;

            ApplyNormalPhysicsProfile();

            if (config.FirstRun)
            {
                notifications.Push("PPG Performance+", "Press F10 to open settings. Defaults favor safety over aggression.", 6f);
                config.FirstRun = false;
                ConfigStore.Save(config);
            }
        }

        private void OnDestroy()
        {
            SaveAll();
        }

        private void Update()
        {
            if (config == null)
            {
                return;
            }

            notifications.Update();
            frameTracker.Record(Time.unscaledDeltaTime * 1000f);

            if (Input.GetKeyDown(settingsKey))
            {
                overlayVisible = !overlayVisible;
            }

            UpdateLagState();
            UpdateEmergencyMode();

            if (Time.frameCount >= nextScanTime)
            {
                latestSnapshot = ScanScene();
                nextScanTime = Time.frameCount + Mathf.Max(1, config.ScanIntervalFrames);
            }
        }

        private void OnGUI()
        {
            notifications.Draw(Screen.width - 340f, 20f, 320f);

            if (!overlayVisible)
            {
                return;
            }

            GUILayout.BeginArea(new Rect(20f, 20f, 400f, 540f), "PPG Performance+", GUI.skin.window);
            GUILayout.Label("F10 toggles this panel.");
            GUILayout.Space(6f);
            GUILayout.Label("Latest frame: " + frameTracker.LatestFrameMs.ToString("0.0") + " ms");
            GUILayout.Label("Average frame: " + frameTracker.GetAverageFrameMs().ToString("0.0") + " ms");
            GUILayout.Label("Worst frame: " + frameTracker.WorstFrameMs.ToString("0.0") + " ms");
            GUILayout.Label("Bodies: " + latestSnapshot.RigidbodyCount + " | Joints: " + latestSnapshot.JointCount + " | Debris: " + latestSnapshot.DebrisCount);
            GUILayout.Space(8f);

            var changed = false;
            changed |= Toggle("Notifications", ref config.ShowNotifications);
            changed |= Toggle("Lag warnings", ref config.EnableLagWarnings);
            changed |= Toggle("Auto-sleep", ref config.EnableAutoSleep);
            changed |= Toggle("Debris control", ref config.EnableDebrisControl);
            changed |= Toggle("Offscreen optimization", ref config.EnableOffscreenOptimization);
            changed |= Toggle("Adaptive physics", ref config.EnableAdaptivePhysics);
            changed |= Toggle("Freeze protection", ref config.EnableFreezeProtection);
            changed |= Toggle("Safe spawn stabilisation", ref config.EnableSafeSpawnStabilisation);
            changed |= Toggle("Remove tiny debris", ref config.RemoveTinyDebris);

            GUILayout.Space(8f);
            GUILayout.Label("Auto-sleep delay: " + config.AutoSleepDelaySeconds.ToString("0.0") + "s");
            config.AutoSleepDelaySeconds = GUILayout.HorizontalSlider(config.AutoSleepDelaySeconds, 1f, 15f);
            GUILayout.Label("Heavy spawn threshold: " + config.HeavySpawnBodyThreshold);
            config.HeavySpawnBodyThreshold = Mathf.RoundToInt(GUILayout.HorizontalSlider(config.HeavySpawnBodyThreshold, 20f, 250f));
            GUILayout.Label("Max debris bodies: " + config.MaxDebrisBodies);
            config.MaxDebrisBodies = Mathf.RoundToInt(GUILayout.HorizontalSlider(config.MaxDebrisBodies, 50f, 600f));

            GUILayout.Space(8f);
            if (GUILayout.Button("Save Settings"))
            {
                SaveAll();
                notifications.Push("Settings saved", "Configuration written to disk.", 2f);
            }

            if (GUILayout.Button("Reset Physics Profile"))
            {
                emergencyModeActive = false;
                ApplyNormalPhysicsProfile();
                notifications.Push("Physics profile", "Returned to the normal physics profile.", 2f);
            }

            if (GUILayout.Button("Write Session Report"))
            {
                SaveSessionReport();
                notifications.Push("Session report", "Wrote the current session report to disk.", 2f);
            }

            if (changed)
            {
                ConfigStore.Save(config);
            }

            GUILayout.EndArea();
        }

        private bool Toggle(string label, ref bool value)
        {
            var next = GUILayout.Toggle(value, label);
            if (next == value)
            {
                return false;
            }

            value = next;
            return true;
        }

        private void UpdateLagState()
        {
            if (frameTracker.LatestFrameMs >= config.SustainedFrameThresholdMs)
            {
                consecutiveLagFrames++;
            }
            else
            {
                consecutiveLagFrames = 0;
            }

            if (!config.EnableLagWarnings)
            {
                return;
            }

            if (consecutiveLagFrames < config.ConsecutiveLagFramesForWarning)
            {
                return;
            }

            if (Time.realtimeSinceStartup - lastNotificationTime < config.NotificationCooldownSeconds)
            {
                return;
            }

            lastNotificationTime = Time.realtimeSinceStartup;
            warningsIssued++;
            if (config.ShowNotifications)
            {
                notifications.Push("Lag warning", "Sustained frame degradation detected. Open F10 settings if you want a less aggressive or more aggressive profile.", 4f);
            }
        }

        private void UpdateEmergencyMode()
        {
            var severe = frameTracker.LatestFrameMs >= config.SevereFrameThresholdMs;
            var sustained = frameTracker.GetAverageFrameMs() >= config.SevereFrameThresholdMs;

            if (config.EnableFreezeProtection && (severe || sustained))
            {
                emergencyModeActive = true;
                emergencyUntilTime = Time.realtimeSinceStartup + config.EmergencyModeSeconds;
            }

            if (emergencyModeActive && Time.realtimeSinceStartup > emergencyUntilTime)
            {
                emergencyModeActive = false;
            }

            if (config.EnableAdaptivePhysics)
            {
                if (emergencyModeActive)
                {
                    ApplyEmergencyPhysicsProfile();
                }
                else if (frameTracker.GetAverageFrameMs() >= config.SustainedFrameThresholdMs || latestSnapshot.JointCount >= 400)
                {
                    ApplyReducedPhysicsProfile();
                }
                else
                {
                    ApplyNormalPhysicsProfile();
                }
            }
        }

        private SceneSnapshot ScanScene()
        {
            var snapshot = new SceneSnapshot();
            var bodies = FindObjectsOfType<Rigidbody2D>();
            var joints = FindObjectsOfType<Joint2D>();
            var now = Time.realtimeSinceStartup;
            var bodyDelta = Mathf.Max(0, bodies.Length - lastRigidbodyCount);

            snapshot.RigidbodyCount = bodies.Length;
            snapshot.JointCount = joints.Length;

            if (snapshot.RigidbodyCount > peakRigidbodies)
            {
                peakRigidbodies = snapshot.RigidbodyCount;
            }

            if (snapshot.JointCount > peakJoints)
            {
                peakJoints = snapshot.JointCount;
            }

            for (int i = 0; i < bodies.Length; i++)
            {
                var body = bodies[i];
                if (body == null)
                {
                    continue;
                }

                var state = GetBodyState(body, now);
                var isVisible = IsBodyVisible(body);
                var isTinyLoose = IsTinyLooseBody(body);

                if (isTinyLoose)
                {
                    snapshot.TinyLooseBodyCount++;
                }

                if (IsDebrisBody(body))
                {
                    snapshot.DebrisCount++;
                }

                UpdateBodyState(body, state, isVisible);
                TryAutoSleep(body, state);
                TryOptimizeOffscreenBody(body, state, isVisible);
                TryControlDebris(body, isTinyLoose, snapshot.DebrisCount);
            }

            CleanupStaleBodyStates(now);
            HandleSpawnSpike(bodyDelta, snapshot);

            lastRigidbodyCount = snapshot.RigidbodyCount;
            lastJointCount = snapshot.JointCount;

            return snapshot;
        }

        private BodyState GetBodyState(Rigidbody2D body, float now)
        {
            var id = body.GetInstanceID();
            BodyState state;

            if (!trackedBodies.TryGetValue(id, out state))
            {
                state = new BodyState();
                trackedBodies[id] = state;
            }

            state.LastSeenTime = now;

            if (!state.HadOriginalInterpolation)
            {
                state.OriginalInterpolation = body.interpolation;
                state.HadOriginalInterpolation = true;
            }

            return state;
        }

        private void UpdateBodyState(Rigidbody2D body, BodyState state, bool isVisible)
        {
            if (body.bodyType != RigidbodyType2D.Dynamic)
            {
                state.IdleTime = 0f;
                state.OffscreenTime = 0f;
                return;
            }

            if (IsProtectedBody(body.gameObject))
            {
                state.IdleTime = 0f;
                state.OffscreenTime = 0f;
                return;
            }

            var speed = body.velocity.magnitude;

            if (speed <= config.IdleVelocityThreshold && Mathf.Abs(body.angularVelocity) <= 3f)
            {
                state.IdleTime += config.ScanIntervalFrames * Time.unscaledDeltaTime;
            }
            else
            {
                state.IdleTime = 0f;
            }

            if (!isVisible)
            {
                state.OffscreenTime += config.ScanIntervalFrames * Time.unscaledDeltaTime;
            }
            else
            {
                state.OffscreenTime = 0f;
            }
        }

        private void TryAutoSleep(Rigidbody2D body, BodyState state)
        {
            if (!config.EnableAutoSleep)
            {
                return;
            }

            if (body.bodyType != RigidbodyType2D.Dynamic || !body.simulated)
            {
                return;
            }

            if (IsProtectedBody(body.gameObject))
            {
                return;
            }

            if (state.IdleTime < config.AutoSleepDelaySeconds)
            {
                return;
            }

            if (!body.IsSleeping())
            {
                body.Sleep();
                autoSleepActions++;
            }
        }

        private void TryOptimizeOffscreenBody(Rigidbody2D body, BodyState state, bool isVisible)
        {
            if (!config.EnableOffscreenOptimization)
            {
                if (state.InterpolationReduced)
                {
                    body.interpolation = state.OriginalInterpolation;
                    state.InterpolationReduced = false;
                }

                return;
            }

            if (body.bodyType != RigidbodyType2D.Dynamic || IsProtectedBody(body.gameObject))
            {
                return;
            }

            if (!isVisible && state.OffscreenTime >= config.OffscreenSleepDelaySeconds)
            {
                if (!state.InterpolationReduced)
                {
                    state.OriginalInterpolation = body.interpolation;
                    body.interpolation = RigidbodyInterpolation2D.None;
                    state.InterpolationReduced = true;
                }

                if (body.velocity.sqrMagnitude <= config.IdleVelocityThreshold * config.IdleVelocityThreshold)
                {
                    body.Sleep();
                }
            }
            else if (state.InterpolationReduced)
            {
                body.interpolation = state.OriginalInterpolation;
                state.InterpolationReduced = false;
            }
        }

        private void TryControlDebris(Rigidbody2D body, bool isTinyLoose, int debrisCount)
        {
            if (!config.EnableDebrisControl)
            {
                return;
            }

            if (!isTinyLoose)
            {
                return;
            }

            if (debrisCount <= config.MaxDebrisBodies && !emergencyModeActive)
            {
                return;
            }

            if (config.RemoveTinyDebris)
            {
                Destroy(body.gameObject);
            }
            else
            {
                if (!body.IsSleeping())
                {
                    body.Sleep();
                }
            }

            debrisActions++;
        }

        private void HandleSpawnSpike(int bodyDelta, SceneSnapshot snapshot)
        {
            if (bodyDelta < config.HeavySpawnBodyThreshold)
            {
                return;
            }

            heavySpawnEvents++;

            if (config.ShowNotifications)
            {
                notifications.Push(
                    "Heavy spawn detected",
                    bodyDelta + " new rigidbodies appeared. Applying the conservative profile temporarily.",
                    4f);
            }

            if (config.EnableSafeSpawnStabilisation)
            {
                emergencyModeActive = true;
                emergencyUntilTime = Time.realtimeSinceStartup + Mathf.Max(2f, config.EmergencyModeSeconds * 0.5f);
            }

            if (bodyDelta >= config.DangerousSpawnBodyThreshold)
            {
                warningsIssued++;
                if (config.ShowNotifications)
                {
                    notifications.Push(
                        "Dangerous spawn level",
                        "This scene spike crossed the dangerous threshold. Consider smaller contraptions or lower spawn density.",
                        5f);
                }
            }
        }

        private void CleanupStaleBodyStates(float now)
        {
            var stale = new List<int>();

            foreach (var pair in trackedBodies)
            {
                if (now - pair.Value.LastSeenTime > 30f)
                {
                    stale.Add(pair.Key);
                }
            }

            for (int i = 0; i < stale.Count; i++)
            {
                trackedBodies.Remove(stale[i]);
            }
        }

        private bool IsBodyVisible(Rigidbody2D body)
        {
            var renderers = body.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].isVisible)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsTinyLooseBody(Rigidbody2D body)
        {
            if (body == null || body.gameObject == null)
            {
                return false;
            }

            if (body.GetComponents<Joint2D>().Length > 0)
            {
                return false;
            }

            if (IsProtectedBody(body.gameObject))
            {
                return false;
            }

            var renderer = body.GetComponent<Renderer>();
            if (renderer != null)
            {
                var bounds = renderer.bounds.size;
                return bounds.x <= config.SmallBodySizeThreshold && bounds.y <= config.SmallBodySizeThreshold;
            }

            var scale = body.transform.lossyScale;
            return scale.x <= config.SmallBodySizeThreshold && scale.y <= config.SmallBodySizeThreshold;
        }

        private bool IsDebrisBody(Rigidbody2D body)
        {
            if (body == null || body.gameObject == null)
            {
                return false;
            }

            return body.gameObject.layer == 10 || body.gameObject.layer == 13 || IsTinyLooseBody(body);
        }

        private bool IsProtectedBody(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return true;
            }

            if (gameObject.GetComponent("LimbBehaviour") != null)
            {
                return true;
            }

            if (gameObject.GetComponent("AliveBehaviour") != null)
            {
                return true;
            }

            if (gameObject.GetComponent("PersonBehaviour") != null)
            {
                return true;
            }

            if (gameObject.CompareTag("Player"))
            {
                return true;
            }

            return false;
        }

        private void ApplyNormalPhysicsProfile()
        {
            Physics2D.velocityIterations = Mathf.Max(1, config.NormalVelocityIterations);
            Physics2D.positionIterations = Mathf.Max(1, config.NormalPositionIterations);
        }

        private void ApplyReducedPhysicsProfile()
        {
            Physics2D.velocityIterations = Mathf.Max(1, config.ReducedVelocityIterations);
            Physics2D.positionIterations = Mathf.Max(1, config.ReducedPositionIterations);
        }

        private void ApplyEmergencyPhysicsProfile()
        {
            Physics2D.velocityIterations = Mathf.Max(1, config.EmergencyVelocityIterations);
            Physics2D.positionIterations = Mathf.Max(1, config.EmergencyPositionIterations);
        }

        private void SaveAll()
        {
            if (config != null)
            {
                ConfigStore.Save(config);
            }

            SaveSessionReport();
        }

        private void SaveSessionReport()
        {
            var builder = new StringBuilder();
            builder.AppendLine("PPG Performance+ Session Report");
            builder.AppendLine("Generated: " + DateTime.Now.ToString("u"));
            builder.AppendLine("Peak rigidbodies: " + peakRigidbodies);
            builder.AppendLine("Peak joints: " + peakJoints);
            builder.AppendLine("Auto-sleep actions: " + autoSleepActions);
            builder.AppendLine("Debris actions: " + debrisActions);
            builder.AppendLine("Heavy spawn events: " + heavySpawnEvents);
            builder.AppendLine("Warnings issued: " + warningsIssued);
            builder.AppendLine("Worst frame time (ms): " + frameTracker.WorstFrameMs.ToString("0.0"));
            builder.AppendLine("Average frame time (ms): " + frameTracker.GetAverageFrameMs().ToString("0.0"));
            builder.AppendLine("Last snapshot rigidbodies: " + latestSnapshot.RigidbodyCount);
            builder.AppendLine("Last snapshot joints: " + latestSnapshot.JointCount);
            builder.AppendLine("Last snapshot debris: " + latestSnapshot.DebrisCount);
            ConfigStore.SaveSessionReport(builder.ToString());
        }

        private KeyCode ParseKey(string keyName)
        {
            try
            {
                return (KeyCode)Enum.Parse(typeof(KeyCode), keyName, true);
            }
            catch
            {
                return KeyCode.F10;
            }
        }
    }
}
