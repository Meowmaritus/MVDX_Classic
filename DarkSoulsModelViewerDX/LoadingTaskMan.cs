using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DarkSoulsModelViewerDX
{
    public static class LoadingTaskMan
    {
        private class LoadingTask
        {
            public string DisplayString;
            public double ProgressRatio { get; private set; }
            public bool IsComplete { get; private set; }
            Thread taskThread;

            public LoadingTask(string displayString, Action<IProgress<double>> doLoad)
            {
                DisplayString = displayString;
                IProgress<double> prog = new Progress<double>(x => ProgressRatio = x);

                taskThread = new Thread(() =>
                {
                    doLoad.Invoke(prog);
                    // We don't check ProgressRatio to see if it's done, since
                    // the thread is INSTANTLY KILLED when complete, which would
                    // cause slight progress rounding errors to destroy the
                    // entire universe. Instead, it's only considered done 
                    // after the entire doLoad is complete.
                    IsComplete = true;
                });

                taskThread.IsBackground = true;
                taskThread.Start();
            }

            public void Kill()
            {
                if (taskThread != null && taskThread.IsAlive)
                    taskThread.Abort();
            }
        }

        private static object _lock_TaskDictEdit = new object();

        private static Dictionary<string, LoadingTask> TaskDict = new Dictionary<string, LoadingTask>();

        /// <summary>
        /// Starts a loading task if that task wasn't already running.
        /// </summary>
        /// <param name="taskKey">String key to reference task by. This is what determines if it's running already.</param>
        /// <param name="displayString">String to actually show onscreen next to the progress bar.</param>
        /// <param name="taskDelegate">The actual task to perform. Be sure to report a progress of 1.0 in the IProgress when done.</param>
        /// <returns>True if the task was just started. False if it was already running.</returns>
        public static bool DoLoadingTask(string taskKey, string displayString, Action<IProgress<double>> taskDelegate)
        {
            lock (_lock_TaskDictEdit)
            {
                if (TaskDict.ContainsKey(taskKey))
                    return false;
                // As soon as the LoadingTask is created it starts.
                // The class is private because that's dangerous if you're an idiot.
                TaskDict.Add(taskKey, new LoadingTask(displayString, taskDelegate));
            }
            
            return true;
        }

        public static bool IsTaskRunning(string taskKey)
        {
            lock (_lock_TaskDictEdit)
            {
                if (TaskDict.ContainsKey(taskKey))
                {
                    // While we're here, might as well double check if that task is done.
                    if (TaskDict[taskKey].IsComplete)
                    {
                        TaskDict[taskKey].Kill();
                        TaskDict.Remove(taskKey);
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        public static void Update(float elapsedTime)
        {
            lock (_lock_TaskDictEdit)
            {
                // Cleanup finished tasks
                var keyList = TaskDict.Keys;
                var keysOfTasksThatAreFinished = new List<string>();
                foreach (var key in keyList)
                {
                    if (TaskDict[key].IsComplete)
                    {
                        TaskDict[key].Kill();
                        keysOfTasksThatAreFinished.Add(key);
                    }
                }
                foreach (var key in keysOfTasksThatAreFinished)
                {
                    TaskDict.Remove(key);
                }
            }
        }

        public static void DrawAllTasks()
        {
            const int DistFromEdgesOfScreen = 8;
            const int DistBetweenProgressRects = 8;
            const int TaskRectWidth = 360;
            const int TaskRectHeight = 64;
            const int ProgBarHeight = 20;
            const int ProgBarDistFromRectEdge = 8;
            const int ProgBarEdgeThickness = 2;
            const float ProgNameDistFromEdge = 8;

            if (TaskDict.Count > 0)
            {
                GFX.SpriteBatch.Begin();

                int i = 0;
                foreach (var kvp in TaskDict)
                {
                    // Draw Task Rect
                    Rectangle thisTaskRect = new Rectangle(
                        GFX.Device.Viewport.Width - TaskRectWidth - DistFromEdgesOfScreen,
                        DistFromEdgesOfScreen + ((DistBetweenProgressRects + TaskRectHeight) * i),
                        TaskRectWidth, TaskRectHeight);

                    GFX.SpriteBatch.Draw(Main.DEFAULT_TEXTURE_DIFFUSE, thisTaskRect, Color.Black * 0.75f);

                    // Draw Progress Background Rect

                    Rectangle progBackgroundRect = new Rectangle(thisTaskRect.X + ProgBarDistFromRectEdge,
                        thisTaskRect.Y + TaskRectHeight - ProgBarDistFromRectEdge - ProgBarHeight,
                        thisTaskRect.Width - (ProgBarDistFromRectEdge * 2), ProgBarHeight);

                    GFX.SpriteBatch.Draw(Main.DEFAULT_TEXTURE_DIFFUSE, progBackgroundRect, Color.DarkGray * 0.85f);

                    // Draw Progress Foreground Rect

                    Rectangle progForegroundRect = new Rectangle(
                        progBackgroundRect.X + ProgBarEdgeThickness,
                        progBackgroundRect.Y + ProgBarEdgeThickness,
                        (int)((progBackgroundRect.Width - (ProgBarEdgeThickness * 2)) * kvp.Value.ProgressRatio),
                        progBackgroundRect.Height - (ProgBarEdgeThickness * 2));

                    GFX.SpriteBatch.Draw(Main.DEFAULT_TEXTURE_DIFFUSE, progForegroundRect, Color.White * 0.95f);

                    // Draw Task Name

                    Vector2 taskNamePos = new Vector2(thisTaskRect.X + ProgNameDistFromEdge, thisTaskRect.Y + ProgNameDistFromEdge);

                    DBG.DrawOutlinedText(kvp.Value.DisplayString, taskNamePos, 
                        Color.White, DBG.DEBUG_FONT_SMALL, startAndEndSpriteBatchForMe: false);

                    i++;
                }

                GFX.SpriteBatch.End();
            }

            
        }
    }
}
