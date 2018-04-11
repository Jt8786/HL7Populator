namespace HL7Populator.Logging
{
    using log4net;
    using log4net.Appender;
    using log4net.Config;
    using log4net.Core;
    using log4net.Layout;
    using log4net.Repository.Hierarchy;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using System.Threading;

    public class Log
    {
        #region Properties and Constructors
        private static object _lockObject = new object();
        private static bool logIsInitialized = false;
        private ILog log { get; set; }
        private int _loggerLevel { get; set; }
        public LogLevel LoggerLevel
        {
            get
            {
                return (LogLevel)this._loggerLevel;
            }
            set
            {
                this._loggerLevel = (int)value;
                switch (this._loggerLevel)
                {
                    case 0:
                        ((log4net.Repository.Hierarchy.Logger)log.Logger).Level = (log4net.Core.Level.Off);
                        break;
                    case 1:
                        ((log4net.Repository.Hierarchy.Logger)log.Logger).Level = (log4net.Core.Level.Fatal);
                        break;
                    case 2:
                        ((log4net.Repository.Hierarchy.Logger)log.Logger).Level = (log4net.Core.Level.Error);
                        break;
                    case 3:
                        ((log4net.Repository.Hierarchy.Logger)log.Logger).Level = (log4net.Core.Level.Warn);
                        break;
                    case 4:
                        ((log4net.Repository.Hierarchy.Logger)log.Logger).Level = (log4net.Core.Level.Info);
                        break;
                    case 5:
                        ((log4net.Repository.Hierarchy.Logger)log.Logger).Level = (log4net.Core.Level.Debug);
                        break;
                    case 6:
                        ((log4net.Repository.Hierarchy.Logger)log.Logger).Level = (log4net.Core.Level.All);
                        break;
                }

            }
        }

        public static Log CoreLogger = new Log("Core");
        public static Log DataLogger = new Log("HL7Populator");
        public static Log NetworkLogger = new Log("Network");
        public static Log HL7Logger = new Log("HL7");
        

        private Log(string logger)
        {
            if (!logIsInitialized)
            {
                lock (_lockObject)
                {
                    Log.ConfigureFileAppender(@"C:\ProgramData\HL7Populator\HL7Populator.log"); // This only has to be called once.
                    logIsInitialized = true;
                }
            }

            log = LogManager.GetLogger(logger);
            log.Info("Initializing logging");

            LoggerLevel = LogLevel.Info;
        }
        #endregion

        #region Enums
        public enum LogLevel
        {
            Off, Fatal, Error, Warn, Info, Debug, All
        }
        #endregion

        #region Methods
        public static void SetLoggingLevel(LogLevel logLevel)
        {
            CoreLogger.LoggerLevel = logLevel;
            DataLogger.LoggerLevel = logLevel;
            NetworkLogger.LoggerLevel = logLevel;
            HL7Logger.LoggerLevel = logLevel;
        }

        public void Info(object text)
        {
            log.Info(text);
            Write(text);
        }

        public void Info(string text, params object[] args)
        {
            string t = string.Format(text, args);
            log.Info(t);
            Write(t);
        }

        public void Debug(string text)
        {
            log.Debug(text);
        }

        public void Debug(string text, params object[] args)
        {
            string t = string.Format(text, args);
            log.Debug(t);
            Write(t);
        }

        public void Error(string text)
        {
            log.Error(text);
        }

        public void Error(string text, params object[] args)
        {
            string t = string.Format(text, args);
            log.Error(t);
            Write(t);
        }

        public void Try(Action action)
        {
            var stack = new StackTrace().GetFrame(1);

            log.Info(string.Format("{0} -  Started", stack.GetMethod().Name));
            Stopwatch sw = new Stopwatch();

            sw.Start();

            string error = null;
            try
            {
                action();
            }
            catch (HL7PopulatorException e)
            {
                error = e.ToString();
                log.Error(e.ToString());
            }
            catch (Exception e)
            {
                error = e.ToString();
                log.Fatal(e.ToString());
                throw;
            }
            finally
            {
                sw.Stop();

                log.Info(string.Format("{0} -  {1}", stack.GetMethod().Name, error != null ? "Threw an Error" : string.Format("Completed successfully in {0} ms", sw.ElapsedMilliseconds)));
            }
        }

        public void Try(Action action, int timeout)
        {
            var stack = new StackTrace().GetFrame(1);

            log.Info(string.Format("{0} -  Started with timeout {1} ms", stack.GetMethod().Name, timeout));

            Stopwatch sw = new Stopwatch();

            sw.Start();

            string error = null;
            try
            {
                Thread threadToKill = null;
                Action wrappedAction = () =>
                {
                    threadToKill = Thread.CurrentThread;
                    action();
                };

                IAsyncResult result = wrappedAction.BeginInvoke(null, null);
                if (result.AsyncWaitHandle.WaitOne(timeout))
                {
                    wrappedAction.EndInvoke(result);
                }
                else
                {
                    threadToKill.Abort();
                    log.Info(string.Format("{0} -  {1}", stack.GetMethod().Name, "Timeout expired"));
                }
            }
            catch (HL7PopulatorException e)
            {
                error = e.ToString();
                log.Error(e.ToString());
            }
            catch (Exception e)
            {
                error = e.ToString();
                log.Fatal(e.ToString());
                throw;
            }
            finally
            {
                sw.Stop();

                log.Info(string.Format("{0} -  {1}", stack.GetMethod().Name, error != null ? "Threw an Error" : string.Format("Completed successfully in {0} ms", sw.ElapsedMilliseconds)));
            }
        }

        private void Write(object text)
        {
            string s = string.Empty;

            foreach (var l in StringExtension.Wrap(text.ToString(), 70))
            {
                if (s == string.Empty)
                    s = l;
                else
                {
                    s += "\r\n" + l.PadLeft(l.Length + 11);
                }
            }

            Console.WriteLine(string.Format("{0} - {1}", DateTime.Now.ToString("HH:mm:ss"), s));
        }
        #endregion

        private class StringExtension
        {
            public static List<string> Wrap(string text, int maxLength)
            {
                if (text.Length == 0)
                    return new List<string>();

                var words = text.Split(' ');
                var lines = new List<string>();
                var currentLine = string.Empty;

                foreach (var currentWord in words)
                {
                    string wordToAdd = currentWord;

                    if (wordToAdd.Contains("\n"))
                    {
                        string[] wordSplit = wordToAdd.Split('\n');

                        for (int i = 0; i < wordSplit.Length; i++)
                        {
                            if ((currentLine.Length > maxLength) || ((currentLine.Length + wordSplit[i].Length > maxLength)))
                            {
                                lines.Add(currentLine);
                                currentLine = string.Empty;
                            }

                            if (i + 1 != wordSplit.Length)
                            {
                                if (currentLine.Length > 0)
                                    currentLine += " " + wordSplit[i];
                                else
                                    currentLine += wordSplit[i];

                                if (currentLine.Length > 0)
                                {
                                    lines.Add(currentLine);
                                    currentLine = string.Empty;
                                }
                            }
                            else
                                wordToAdd = wordSplit[i];
                        }
                    }
                    else if ((currentLine.Length > maxLength) || ((currentLine.Length + wordToAdd.Length > maxLength)))
                    {
                        lines.Add(currentLine);
                        currentLine = string.Empty;
                    }

                    if (currentLine.Length > 0)
                        currentLine += " " + wordToAdd;
                    else
                        currentLine += wordToAdd;
                }

                if (currentLine.Length > 0)
                    lines.Add(currentLine);

                return lines;
            }
        }

        #region log4net Configuration
        private static void ConfigureFileAppender(string logFile)
        {
            var fileAppender = GetFileAppender(logFile);
            BasicConfigurator.Configure(fileAppender);
            ((Hierarchy)LogManager.GetRepository()).Root.Level = Level.Debug;
        }

        private static IAppender GetFileAppender(string logFile)
        {
            var layout = new PatternLayout("%date %-5level %logger %t - %message%newline");
            layout.ActivateOptions(); // According to the docs this must be called as soon as any properties have been changed.

            var appender = new FileAppender
            {
                File = logFile,
                Encoding = Encoding.UTF8,
                Threshold = Level.Debug,
                Layout = layout
            };

            appender.ActivateOptions();

            return appender;
        }
        #endregion
    }
}
