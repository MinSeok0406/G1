using AccountServer.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedDB;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace AccountServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IDatabase _cache;
        SharedDbContext _shared;

        public AccountController(AppDbContext context, IConnectionMultiplexer mux , SharedDbContext shared)
        {
            _context = context;
            _cache = mux.GetDatabase();
            _shared = shared;
        }

        [HttpPost]
        [Route("create")]
        public CreateAccountPacketRes CreateAccount([FromBody] CreateAccountPacketReq req)
        {
            Console.WriteLine($"[AccountController] /api/account/create HIT. {req?.GoogleID}");

            CreateAccountPacketRes res = new CreateAccountPacketRes();

            AccountDb account = _context.Accounts
                                    .AsNoTracking()
                                    .Where(a => a.GoogleID == req.GoogleID)
                                    .FirstOrDefault();

            if (account == null)
            {
                _context.Accounts.Add(new AccountDb()
                {
                    GoogleID = req.GoogleID,
                    Name = req.AccountName
                });

                res.CreateOk = _context.SaveChangesEx();
            }
            else
            {
                res.CreateOk = false;
                Console.WriteLine("already created");
            }

            return res;
        }

        [HttpPost]
        [Route("login")]
        public LoginAccountPacketRes LoginAccount([FromBody] LoginAccountPacketReq req)
        {
            LoginAccountPacketRes res = new LoginAccountPacketRes();

            AccountDb account = _context.Accounts
                                    .AsNoTracking()
                                    .Where(a => a.GoogleID == req.GoogleID)
                                    .FirstOrDefault();

            if (account == null)
            {
                res.LoginOk = false;
            }
            else
            {
                res.LoginOk = true;
                int newToken = RandomNumberGenerator.GetInt32(int.MaxValue);

                // ① 토큰 생성/DB 저장(SharedDB.Token)
                var expired = DateTime.UtcNow.AddHours(12);

                TokenDb tokenDb = _shared.Tokens.Where(t => t.AccountDbId == account.AccountDbId).FirstOrDefault();
                if (tokenDb != null)
                {
                    tokenDb.Token = newToken;
                    tokenDb.Expired = expired;
                    _shared.SaveChangesEx();
                }
                else
                {
                    tokenDb = new TokenDb()
                    {
                        AccountDbId = account.AccountDbId,
                        Name = account.Name,
                        GoogleID = account.GoogleID,
                        Token = newToken,
                        Expired = expired
                    };
                    _shared.Add(tokenDb);
                    _shared.SaveChangesEx();
                }

                // ② Redis 캐시(유효기간 동기화)
                var ttl = expired - DateTime.UtcNow;
                if (ttl.TotalSeconds > 0)
                {
                    _cache.StringSet($"acc:token:{account.AccountDbId}", tokenDb.Token.ToString(), ttl);
                    _cache.StringSet($"acc:name:{account.AccountDbId}", account.Name ?? "", ttl);
                }

                res.AccountId = account.AccountDbId;
                res.Token = tokenDb.Token;
                res.Name = tokenDb.Name;
                res.GoogleID = tokenDb.GoogleID;

                res.ServerList = _shared.Servers.AsNoTracking()
                    .Select(s => new ServerInfo
                    {
                        Name = s.Name,
                        IpAddress = s.IpAddress,
                        Port = s.Port,
                        BusyScore = s.BusyScore
                    }).ToList();
            }

            return res;
        }
    }
}
