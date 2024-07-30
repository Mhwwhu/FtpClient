using Client.Database.Entity;
using Client.Logger;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Client.Database
{
	public class DbController
	{
		private readonly RecordContext _context;
		private readonly ILoggerService _logger;
		private readonly string _tag = "DbController";
		private ReaderWriterLockSlim _lock = new();
		public DbController(IConfiguration configuration, ILoggerService logger)
		{
			DbContextOptionsBuilder<RecordContext> optionsBuilder = new();
			optionsBuilder.UseSqlite(configuration.GetSection("Database")["ConnectionString"]);
			optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
			_context = new RecordContext(optionsBuilder.Options);
			//_context.Database.Migrate();
			_logger = logger;
			_context.Database.EnsureCreated();
		}

		public int AddServer(string hostIp, string hostName)
		{
			try
			{
				var server = new FtpServerEntity
				{
					IpAddress = hostIp,
					Username = hostName
				};
				_lock.EnterWriteLock();
				_context.Servers.Add(server);
				_context.SaveChanges();
				return server.Id;
			}
			catch (Exception ex)
			{
				_logger.Error(_tag, "Add server failed");
				_logger.Error(_tag, ex.Message);
				throw;
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}
		
		public int AddRecord(int? serverId, string command)
		{
			var record = new RecordEntity
			{
				ServerId = serverId,
				Command = command,
				Timestamp = DateTime.Now
			};
			try
			{
				_lock.EnterWriteLock();
				_context.Records.Add(record);
				_context.SaveChanges();
				return record.Id;
			}
			catch (Exception ex)
			{
				_logger.Error(_tag, "Add record failed");
				_logger.Error(_tag, ex.Message);
				throw;
			}
			finally { _lock.ExitWriteLock(); }
		}

		public FtpServerEntity? GetServerById(int serverId)
		{
			try
			{
				_lock.EnterReadLock();
				var server = _context.Servers.FirstOrDefault(s => s.Id == serverId);
				return server;
			}
			catch (Exception ex)
			{
				_logger.Error(_tag, "Get server by ID failed");
				_logger.Error(_tag, ex.Message);
				throw;
			}
			finally { _lock.ExitReadLock(); }
		}
		public FtpServerEntity? GetServerByIp(string hostIp, string hostName)
		{
			try
			{
				_lock.EnterReadLock();
				var server = _context.Servers.FirstOrDefault(s => s.IpAddress == hostIp && s.Username == hostName);
				return server;
			}
			catch (Exception ex)
			{
				_logger.Error(_tag, "Get server by IP failed");
				_logger.Error(_tag, ex.Message);
				throw;
			}
			finally { _lock.ExitReadLock(); }
		}

		public List<RecordEntity> GetRecordsById(int startId, int endId)
		{
			try
			{
				_lock.EnterReadLock();
				var records = _context.Records.Where(r => r.Id >= startId && r.Id <= endId).ToList();
				return records;
			}
			catch (Exception ex)
			{
				_logger.Error(_tag, "Get records by ID failed");
				_logger.Error(_tag, ex.Message);
				throw;
			}
			finally { _lock.ExitReadLock(); }
		}
		public List<RecordEntity> GetRecordsByTime(DateTime startTime, DateTime endTime)
		{
			try
			{
				_lock.EnterReadLock();
				var records = _context.Records.Where(r => r.Timestamp >= startTime&& r.Timestamp <= endTime).ToList();
				return records;
			}
			catch (Exception ex)
			{
				_logger.Error(_tag, "Get records by ID failed");
				_logger.Error(_tag, ex.Message);
				throw;
			}
			finally { _lock.ExitReadLock(); }
		}
	}
}
