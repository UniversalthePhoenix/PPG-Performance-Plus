using System.Collections.Generic;

namespace PPGPerformancePlusMod
{
    public class FrameTracker
    {
        private readonly Queue<float> samples = new Queue<float>();
        private readonly int maxSamples;

        public float LatestFrameMs;
        public float WorstFrameMs;

        public FrameTracker(int maxSamples)
        {
            this.maxSamples = maxSamples;
        }

        public void Record(float frameMs)
        {
            LatestFrameMs = frameMs;

            if (frameMs > WorstFrameMs)
            {
                WorstFrameMs = frameMs;
            }

            samples.Enqueue(frameMs);

            while (samples.Count > maxSamples)
            {
                samples.Dequeue();
            }
        }

        public float GetAverageFrameMs()
        {
            if (samples.Count == 0)
            {
                return 0f;
            }

            float total = 0f;
            foreach (var sample in samples)
            {
                total += sample;
            }

            return total / samples.Count;
        }
    }
}
