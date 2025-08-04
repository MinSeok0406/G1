using AccountServer.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedDB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AccountServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        AppDbContext _context;
        SharedDbContext _shared;

        public AccountController(AppDbContext context, SharedDbContext shared)
        {
            _context = context;
            _shared = shared;
        }

        [HttpPost]
        [Route("create")]
        public CreateAccountPacketRes CreateAccount([FromBody] CreateAccountPacketReq req)
        {
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

                bool success = _context.SaveChangesEx();
                res.CreateOk = success;
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

                // 토큰 발급
                DateTime expired = DateTime.UtcNow;
                expired.AddSeconds(600);

                TokenDb tokenDb = _shared.Tokens.Where(t => t.AccountDbId == account.AccountDbId).FirstOrDefault();
                if (tokenDb != null)
                {
                    tokenDb.Token = new Random().Next(Int32.MinValue, Int32.MaxValue);
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
                        Token = new Random().Next(Int32.MinValue, Int32.MaxValue),
                        Expired = expired
                    };
                    _shared.Add(tokenDb);
                    _shared.SaveChangesEx();
                }

                res.AccountId = account.AccountDbId;
                res.Token = tokenDb.Token;
                res.Name = tokenDb.Name;
                res.GoogleID = tokenDb.GoogleID;
                res.ServerList = new List<ServerInfo>();

                foreach (ServerDb serverDb in _shared.Servers)
                {
                    res.ServerList.Add(new ServerInfo()
                    {
                        Name = serverDb.Name,
                        IpAddress = serverDb.IpAddress,
                        Port = serverDb.Port,
                        BusyScore = serverDb.BusyScore
                    });
                }
            }

            return res;
        }
    }
}
