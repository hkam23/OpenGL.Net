
// Copyright (C) 2011-2016 Luca Piccioni
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301
// USA

using System;
using System.IO;

using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Repository;
using log4net.Repository.Hierarchy;

namespace OpenGL.Objects
{
	/// <summary>
	/// Main class for controlling logging, wrapping log4net.
	/// </summary>
	public static class Log
	{
		#region Log Verbosity

		/// <summary>
		/// Log verbosity level.
		/// </summary>
		public enum Verbosity 
		{
			/// <summary>
			/// No log.
			/// </summary>
			Off = 0,
			/// <summary>
			/// Log fatal errors.
			/// </summary>
			Fatal,
			/// <summary>
			/// Log errors or more important events.
			/// </summary>
			Error,
			/// <summary>
			/// Log warnings or more important events.
			/// </summary>
			Warn,
			/// <summary>
			/// Log information or more important events.
			/// </summary>
			Info,
			/// <summary>
			/// Log debug information or more important events.
			/// </summary>
			Debug,
			/// <summary>
			/// Log everything.
			/// </summary>
			Verbose
		}

		/// <summary>
		/// ConvertItemType a <see cref="log4net.Core.Level"/> in <see cref="Verbosity"/>.
		/// </summary>
		/// <param name="level">
		/// A <see cref="log4net.Core.Level"/> to be converted into <see cref="Verbosity"/>.
		/// </param>
		/// <returns>
		/// It returns a <see cref="Verbosity"/> corresponding to <paramref name="level"/>.
		/// </returns>
		private static Verbosity Convert(Level level)
		{
			if (level <= Level.Verbose)
				return (Verbosity.Verbose);
			else if (level <= Level.Debug)
				return (Verbosity.Debug);
			else if (level <= Level.Info)
				return (Verbosity.Info);
			else if (level <= Level.Warn)
				return (Verbosity.Warn);
			else if (level <= Level.Error)
				return (Verbosity.Error);
			else if (level <= Level.Fatal)
				return (Verbosity.Fatal);
			else
				return (Verbosity.Off);
		}

		/// <summary>
		/// ConvertItemType a <see cref="Verbosity"/> in <see cref="log4net.Core.Level"/>.
		/// </summary>
		/// <param name="verbosity">
		/// A <see cref="Verbosity"/> to be converted into <see cref="log4net.Core.Level"/>.
		/// </param>
		/// <returns>
		/// It returns a <see cref="log4net.Core.Level"/> corresponding to <paramref name="verbosity"/>.
		/// </returns>
		private static Level Convert(Verbosity verbosity)
		{
			switch (verbosity) {
				case Verbosity.Off:
					return (Level.Off);
				case Verbosity.Verbose:
					return (Level.Verbose);
				case Verbosity.Debug:
					return (Level.Debug);
				case Verbosity.Info:
					return (Level.Info);
				case Verbosity.Warn:
					return (Level.Warn);
				case Verbosity.Error:
					return (Level.Error);
				case Verbosity.Fatal:
					return (Level.Fatal);
				default:
					return (Level.Off);
			}
		}

		/// <summary>
		/// Set the overall log verbosity.
		/// </summary>
		/// <param name="verbLevel"></param>
		public static void SetLogVerbosity(Verbosity verbLevel)
		{
			ILoggerRepository[] repositories= LogManager.GetAllRepositories();

			foreach (ILoggerRepository repository in repositories) {
				log4net.Core.ILogger[] loggers = ((Hierarchy)repository).GetCurrentLoggers();
				Level logLevel = Level.All;

				switch (verbLevel) {
					case Verbosity.Off:
						logLevel = Level.Off;
						break;
					case Verbosity.Verbose:
						logLevel = Level.Verbose;
						break;
					case Verbosity.Debug:
						logLevel = Level.Debug;
						break;
					case Verbosity.Info:
						logLevel = Level.Info;
						break;
					case Verbosity.Warn:
						logLevel = Level.Warn;
						break;
					case Verbosity.Error:
						logLevel = Level.Error;
						break;
					case Verbosity.Fatal:
						logLevel = Level.Fatal;
						break;
				}

				// Set repository threshold level
				repository.Threshold = repository.LevelMap.LookupWithDefault(logLevel);
				// Set repository liggers threshold level
				foreach (log4net.Core.ILogger logger in loggers)
					((log4net.Repository.Hierarchy.Logger)logger).Level = ((Hierarchy)repository).LevelMap.LookupWithDefault(logLevel);
			}
		}

		#endregion

		#region Logger Configuration

		/// <summary>
		/// Configure the logging system.
		/// </summary>
		public static void Configure()
		{
			Configure(LogFlags.None);
		}

		/// <summary>
		/// Configure the logging system.
		/// </summary>
		public static void Configure(LogFlags flags)
		{
			// Configure base system
			XmlConfigurator.Configure();

			// Log OpenGL method calls
			KhronosApi.RegisterApplicationLogDelegate(delegate(string format, object[] args) {
				_ProcedureLogger.Debug(format, args);
			});
			// Enabe procedure logging
			KhronosApi.LogEnabled = true;
		}

		/// <summary>
		/// Configure the loggin system.
		/// </summary>
		/// <param name="path">
		/// A <see cref="String"/> that specify the log4net configuration file.
		/// </param>
		public static void Configure(string path)
		{
#if DEBUG
			XmlConfigurator.ConfigureAndWatch(new FileInfo(path));
#else
			XmlConfigurator.Configure(new FileInfo(path));
#endif
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		public static void ConfigureLogFile(string path)
		{
			RollingFileAppender rFileAppender = (RollingFileAppender)Array.Find(LogManager.GetRepository().GetAppenders(), delegate(IAppender appender) {
				return ((appender.Name == "RollingFileAppender") && (appender is RollingFileAppender));
			});
			
			if (rFileAppender != null) {
				// Store log file path
				rFileAppender.File = path;
				rFileAppender.ActivateOptions();
			}
		}

		/// <summary>
		/// Create a logger for the specified type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static ILogger GetLogger(Type type) { return (new Logger(type)); }
		
		/// <summary>
		/// The logger used for logging OpenGL procedures.
		/// </summary>
		private static ILogger _ProcedureLogger = GetLogger(typeof(Gl));
		
		#endregion
	}
}
