using System.Collections.Generic;
using UnityEngine;

namespace PPGPerformancePlusMod
{
    public class NotificationCenter
    {
        private class Notice
        {
            public string Title;
            public string Message;
            public float ExpiresAt;
        }

        private readonly List<Notice> notices = new List<Notice>();

        public void Push(string title, string message, float lifetimeSeconds)
        {
            notices.Add(new Notice
            {
                Title = title,
                Message = message,
                ExpiresAt = Time.realtimeSinceStartup + lifetimeSeconds
            });

            Debug.Log("[PPG Performance+] " + title + ": " + message);
        }

        public void Update()
        {
            for (int i = notices.Count - 1; i >= 0; i--)
            {
                if (Time.realtimeSinceStartup >= notices[i].ExpiresAt)
                {
                    notices.RemoveAt(i);
                }
            }
        }

        public void Draw(float startX, float startY, float width)
        {
            var y = startY;

            for (int i = 0; i < notices.Count; i++)
            {
                var rect = new Rect(startX, y, width, 54f);
                GUI.Box(rect, notices[i].Title + "\n" + notices[i].Message);
                y += 58f;
            }
        }
    }
}
