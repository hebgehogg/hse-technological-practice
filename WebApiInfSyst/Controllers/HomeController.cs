using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApiInfSyst.Models;
using WebApiInfSyst.DBwablon;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using ClosedXML.Excel;
using System.IO;
using Microsoft.EntityFrameworkCore.Internal;

namespace WebApiInfSyst.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppCtx _ctx;
        private StoreForm _sf;

        public HomeController(ILogger<HomeController> logger, AppCtx ctx, StoreForm sf)//, User us, StoreForm sf
        {
            _logger = logger;
            _ctx = ctx;
            _sf = sf;
        }
        [HttpPost]
        public JsonResult GIWSearchJson(string GIWval, string GIWname, string GIWgenres, string GIWorder)
        {
            if (GIWgenres == null) GIWgenres = "";
            if (GIWname == null) GIWname = "";
            return Json(GetList(GIWval,GIWname,GIWorder,GIWgenres));
        }
        public async Task<IActionResult> General()
        {
            var res = await _ctx.Games.Where(c => c.Prt != 0).OrderByDescending(c => c.Prt).Take(5).Select(c => new GamesWablon(c.GameID, c.GameName, c.GameCover, c.CountGameBay, c.DeveloperName, c.Prt)).ToListAsync();
            return View(new GeneralInf(res.Count() != 0 ? res : await _ctx.Games.OrderBy(c => c.GameName).Take(1).Select(c => new GamesWablon(c.GameID, c.GameName, c.GameCover, c.CountGameBay, c.DeveloperName, c.Prt)).ToListAsync()));
        }
        public async Task<IActionResult> Lib()
        {
            var cLogin = HttpContext.Request.Cookies["ClientLogin"];
            var res1 = await _ctx.Client
                .Where(c => c.ClientLogin == cLogin)
                .Join(_ctx.ClientsGames,
                c => c.ClientID,
                cg => cg.ClientID,
                (c, cg) => new {
                    GameID = cg.GameID
                }).Join(_ctx.Games,
                cg => cg.GameID,
                g => g.GameID,
                (cg, g) => new {
                    GameID = cg.GameID,
                    GameName = g.GameName,
                    GameDev = g.DeveloperName
                }).ToListAsync();
            var libCont = new List<LibConst>();
            foreach (var item in res1) {
                var res2 = await _ctx.Client
                    .Where(c => c.ClientLogin == cLogin)
                    .Join(_ctx.ClientsInventory,
                    c => c.ClientID,
                    ci => ci.ClientID,
                    (c, ci) => new
                    {
                        InventoryID = ci.InventoryID
                    }).Join(_ctx.Inventory,
                    ci => ci.InventoryID,
                    i => i.InventoryID,
                    (ci, i) => new
                    {
                        InventoryName = i.InventoryName,
                        GameID = i.GameID
                    })
                    .Where(c=>c.GameID==item.GameID)
                    .Select(c=>c.InventoryName).ToListAsync();
                libCont.Add(new LibConst(item.GameName, item.GameDev, res2));
            }
            return View(new GetLib(libCont));
        }
        public async Task<IActionResult> Account()
        {
            var cLogin = HttpContext.Request.Cookies["ClientLogin"];
            var client = await _ctx.Client.Where(c => c.ClientLogin == cLogin).FirstOrDefaultAsync();
            var lcg = await _ctx.ClientsGames.Where(c => c.ClientID == client.ClientID).CountAsync();
            var lci = await _ctx.ClientsInventory.Where(c => c.ClientID == client.ClientID).CountAsync();
            var clientWall = await _ctx.ClientsWallpapers.Where(c => c.ClientID == client.ClientID)
                .Join(_ctx.Wallpapers,
                    c => c.WallpaperID,
                    w => w.WallpaperID,
                    (c, w) => new {
                        WallpaperID = w.WallpaperID,
                        Wallpaperphoto = w.WallpaperPhoto
                    }).Select(c=>new WallAcc(c.WallpaperID,c.Wallpaperphoto)).ToListAsync();
            var clientMWall = clientWall.Where(c => c.WallpaperID == client.WallpaperID)
                .Select(c => c.WallpaperPhoto).FirstOrDefault();
            return View(new AccForm(lcg, lci, clientMWall, clientWall));
        }
        [HttpPost]
        [Obsolete]
        public async Task<IActionResult> loginForm(Client us)
        {
            var res1 = _ctx.Client
                .Where(c => 
                    c.ClientLogin == us.ClientLogin&&
                    c.ClientPassword == ComputeSha256Hash(us.ClientPassword))
                .FirstOrDefault();
            var res2 = _ctx.Staff
                .Where(c =>
                    c.StaffLogin == us.ClientLogin &&
                    c.StaffPasword == ComputeSha256Hash(us.ClientPassword))
                .FirstOrDefault();
            if (res1 != null) {
                var comps = await _ctx.Database.ExecuteSqlCommandAsync(
                    $"insert into ClientStatus(ClientID,StatusStart)" +
                    $"Values(@ClientID,GETDATE());",
                    new SqlParameter("@ClientID", res1.ClientID)
                );
                reloadAcc(res1.ClientLogin, res1.cash);
                Response.Cookies.Append("ClientType", $"U");
                Response.Cookies.Delete("Error");
                return RedirectToAction("General");
            }
            else if (res2 != null) {
                Response.Cookies.Append("ClientLogin", $"{res2.StaffLogin}");
                Response.Cookies.Append("ClientType", $"A");
                Response.Cookies.Delete("Error");
                return RedirectToAction("Manager");
            }
            else {
                Response.Cookies.Append("Error", $"Неверный логин или пароль",
                    new CookieOptions() { Expires = DateTime.UtcNow.AddSeconds(10) });
                return RedirectToAction("Login");
            }
        }
        public IActionResult LogOut()
        {
            setoffline();
            Response.Cookies.Delete("ClientLogin");
            Response.Cookies.Delete("ClientCash");
            Response.Cookies.Delete("ClientType");
            return RedirectToAction("General");
        }
        private void setoffline()
        {
            var type = HttpContext.Request.Cookies["ClientType"];
            var cLogin = HttpContext.Request.Cookies["ClientLogin"];
            if (type != null && type != "M" && type != "S" && type != "A") {
                var upStatus1 = _ctx.Client.Where(c => c.ClientLogin == cLogin).FirstOrDefault();
                upStatus1.ClientStatus = "offline";
                var upStatus2 = _ctx.ClientStatus.Where(c => c.ClientID == upStatus1.ClientID).OrderByDescending(c => c.СlienStatusID).FirstOrDefault();
                upStatus2.StatusEnd = DateTime.UtcNow;
                _ctx.SaveChanges();
            }
        }
        static string ComputeSha256Hash(string rawData)
        {
            string salt = "regarding";
            rawData += salt;
            using (SHA256 sha256Hash = SHA256.Create()) {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                    builder.Append(bytes[i].ToString("x2"));
                return builder.ToString();
            }
        }
        public IActionResult Login()
        {
            return View();
        }
        public IActionResult Registration()
        {
            return View();
        }
        [HttpPost]
        [Obsolete]
        public async Task<IActionResult> regForm(Client us)
        {
            var res = await _ctx.Client.Where(c => c.ClientLogin == us.ClientLogin).CountAsync();
            var res2 = await _ctx.Staff.Where(c=>c.StaffLogin== us.ClientLogin).CountAsync();
            if (res == 0 && res2 == 0) {
                try {
                    var res1 = await _ctx.Database.ExecuteSqlCommandAsync(
                        $"insert into Client(ClientName,ClientLogin,ClientPassword,ClientStatus,ClientRegistrationDate)" +
                        $"values(@Name,@Login,@Pass,@Status,@Date);",
                        new SqlParameter("@Name", us.ClientName),
                        new SqlParameter("@Login", us.ClientLogin),
                        new SqlParameter("@Pass", ComputeSha256Hash(us.ClientPassword)),
                        new SqlParameter("@Status", "offline"),
                        new SqlParameter("@Date", DateTime.UtcNow));
                    Response.Cookies.Delete("Error");
                    return RedirectToAction("Login");
                }
                catch (Exception ex) { return BadRequest(new { message = $"{ex}" }); }
            }
            else {
                Response.Cookies.Append("Error", $"Неверный логин или пароль",
                    new CookieOptions() { Expires = DateTime.UtcNow.AddSeconds(10) });
                return RedirectToAction("Registration"); 
            }
        }
        public IActionResult Store()
        {
            return View(GetList("G","","Name",""));
        }
        private List<InvListStore> GetInv(string GIWname, string GIWorder)
        {
            var currDate = DateTime.Now;
            var invList = _ctx.Inventory
                .Join(_ctx.InventoryIPrices,
                    i => i.InventoryID,
                    ip => ip.InventoryID,
                    (i, ip) => new {
                        InvID = i.InventoryID,
                        Inventoryname = i.InventoryName,
                        GameID = i.GameID,
                        InventoryIPrice = ip.InventoryIPrice,
                        InventoryStartDate = ip.InventoryStartDate,
                        InventoryEndDate = ip.InventoryEndDate
                    }).Where(c => c.InventoryStartDate < currDate && c.InventoryEndDate > currDate)
                .Join(_ctx.Games,
                    i => i.GameID,
                    g => g.GameID,
                    (i, g) => new {
                        Inventoryname = i.Inventoryname,
                        Gamename = g.GameName,
                        InventoryIPrice = i.InventoryIPrice
                    });
            if (GIWname != "") invList = invList.Where(c => c.Gamename == GIWname || c.Inventoryname == GIWname);
            switch (GIWorder)
            {
                default:
                    invList = invList.OrderBy(c => c.Inventoryname);
                    break;
                case "PricePlus":
                    invList = invList.OrderBy(c => c.InventoryIPrice).ThenBy(c => c.Inventoryname);
                    break;
                case "PriceMinus":
                    invList = invList.OrderByDescending(c => c.InventoryIPrice).ThenBy(c => c.Inventoryname);
                    break;
            }
            return invList.Select(a=> new InvListStore(a.Inventoryname, a.Gamename, a.InventoryIPrice)).ToList();
        }
        private List<GamesListStore> GetGames(string GIWname, string GIWorder, string GIWgenres)
        {
            string[] b = null;
            if(GIWgenres!="") b = GIWgenres.Split(':');
            var currDate = DateTime.Now;
            var gameList = _ctx.GemesPrices.Where(c => c.GameStartDate <= currDate && c.GameEndDate >= currDate)
                .Join(_ctx.Games,
                gp => gp.GameID,
                g => g.GameID,
                (gp, g) => new {
                    Price = gp.GamePrice,
                    Gamename = g.GameName,
                    Gamecover = g.GameCover,
                    GameID = g.GameID,
                    Count = g.CountGameBay,
                    Dev = g.DeveloperName
                })
                .Join(_ctx.GameGenre,
                g => g.GameID,
                gg => gg.GameID,
                (g, gg) => new {
                    Price = g.Price,
                    Gamename = g.Gamename,
                    Gamecover = g.Gamecover,
                    genreID = gg.GenreID,
                    Count = g.Count,
                    Dev = g.Dev
                })
                .Join(_ctx.Genres,
                lg => lg.genreID,
                g => g.GenreID,
                (lg, g) => new {
                    Gamename = lg.Gamename,
                    Gamecover = lg.Gamecover,
                    Price = lg.Price,
                    Genre = g.GenreName,
                    Count = lg.Count,
                    Dev = lg.Dev
                });
            if (GIWname != "") gameList = gameList.Where(c => c.Gamename == GIWname);
            switch (GIWorder) {
                case "PricePlus":
                    gameList = gameList.OrderBy(c => c.Price).ThenBy(c => c.Gamename);
                    break;
                case "PriceMinus":
                    gameList = gameList.OrderByDescending(c => c.Price).ThenBy(c => c.Gamename);
                    break;
                default:
                    gameList = gameList.OrderBy(c => c.Gamename);
                    break;
            }
            List<GamesListStore> aaa = new List<GamesListStore>();
            var gn = new List<string>();
            var gc = new List<byte[]>();
            var gp = new List<int>();
            var gd = new List<string>();
            var gg = new List<string>();
            foreach (var a in gameList.ToList()) {
                if (!gn.Contains(a.Gamename)) {
                    gn.Add(a.Gamename);
                    gc.Add(a.Gamecover);
                    gp.Add(a.Price);
                    gd.Add(a.Dev);
                    gg.Add(a.Genre);
                }
                else gg[gg.Count - 1] += $", {a.Genre}";
            }
            for (int i = 0; i < gn.Count; i++)
                if (b==null) aaa.Add(new GamesListStore(gn[i], gc[i],gd[i],gp[i], gg[i]));
                else if (addornor(gg[i],b)) aaa.Add(new GamesListStore(gn[i], gc[i], gd[i], gp[i], gg[i]));
            return aaa;
        }
        private List<WallListStore> GetWal(string GIWname, string GIWorder)
        {
            var currDate = DateTime.Now;
            var wallList = _ctx.Wallpapers
            .Join(_ctx.WallpaperPrice,
                w => w.WallpaperID,
                wp => wp.WallpaperID,
                (w, wp) => new {
                    Wallpapername = w.WallpaperName,
                    Wallpaperphoto = w.WallpaperPhoto,
                    WallpaperPrice = wp.WallpaperPrice1,
                    WallS = wp.WallpaperStartDate,
                    WallE = wp.WallpaperEndDate
                }).Where(c => c.WallS < currDate && c.WallE > currDate);
            if (GIWname != "") wallList = wallList.Where(c => c.Wallpapername == GIWname);
            switch (GIWorder) {
                case "PricePlus":
                    wallList = wallList.OrderBy(c => c.WallpaperPrice).ThenBy(c => c.Wallpapername);
                    break;
                case "PriceMinus":
                    wallList = wallList.OrderByDescending(c => c.WallpaperPrice).ThenBy(c => c.Wallpapername);
                    break;
                default:
                    wallList = wallList.OrderBy(c => c.Wallpapername);
                    break;
            }
            return wallList.Select(a => new WallListStore(a.Wallpaperphoto, a.Wallpapername, a.WallpaperPrice)).ToList();
        }
        private bool addornor(string s,string[]b)
        {
            for (int j = 0; j < b.Length; j++)
                if (!s.Contains(b[j] as string)) return false;
            return true;
        }
        private StoreList GetList(string GIWval, string GIWname, string GIWorder, string GIWgenres)
        {
            switch (GIWval) {
                case "W":
                    var resW = GetWal(GIWname, GIWorder);
                    return new StoreList("W", resW);
                case "I":
                    var resI = GetInv(GIWname, GIWorder);
                    return new StoreList("I", resI);
                default:
                    var resgG = _ctx.Genres.ToList();
                    var resG = GetGames(GIWname,GIWorder,GIWgenres);
                    return new StoreList("G", resG, resgG);
            }
        }
        public IActionResult Spravka()
        {
            return View();
        }
        [HttpPost]
        [Obsolete]
        public async Task<IActionResult> addMny(int cash)
        {
            var cLogin = HttpContext.Request.Cookies["ClientLogin"];
            var comps = await _ctx.Database.ExecuteSqlCommandAsync(
                $"insert into addMny(ClientID,addMnyDate,addMnyCount)" +
                $"select ClientID = (select ClientID from Client where ClientLogin = @Login), " +
                $"getdate(), @cash",
                new SqlParameter("@Login", cLogin),
                new SqlParameter("@cash",cash));
            var res1 = await _ctx.Client.Where(c => c.ClientLogin == cLogin).FirstOrDefaultAsync();
            reloadAcc(res1.ClientLogin, res1.cash);
            return RedirectToAction("RetBack");
        }
        public IActionResult RetBack()
        {
            return View();
        }
        public async Task<IActionResult> changeLogin(string ClientLogin)
        {
            if(await _ctx.Client.Where(c=>c.ClientLogin==ClientLogin).CountAsync()>0||
               await _ctx.Staff.Where(c => c.StaffLogin == ClientLogin).CountAsync() > 0) {
                Response.Cookies.Append("Error", $"Неверный логин или пароль",
                    new CookieOptions() { Expires = DateTime.UtcNow.AddSeconds(2) });
                return RedirectToAction("RetBack");
            }
            var cLogin = HttpContext.Request.Cookies["ClientLogin"];
            var res1 = await _ctx.Client.Where(c => c.ClientLogin == cLogin).FirstOrDefaultAsync();
            res1.ClientLogin = ClientLogin;
            await _ctx.SaveChangesAsync();
            reloadAcc(res1.ClientLogin,res1.cash);
            return RedirectToAction("RetBack");
        }
        public async Task<IActionResult> changePass(string ClientPassword)
        {
            var cLogin = HttpContext.Request.Cookies["ClientLogin"];
            var res1 = await _ctx.Client.Where(c => c.ClientLogin == cLogin).FirstOrDefaultAsync();
            res1.ClientPassword = ComputeSha256Hash(ClientPassword);
            await _ctx.SaveChangesAsync();
            return RedirectToAction("RetBack");
        }
        public async Task<IActionResult> changeWall(int WallpaperID)
        {
            var cLogin = HttpContext.Request.Cookies["ClientLogin"];
            var res1 = await _ctx.Client.Where(c => c.ClientLogin == cLogin).FirstOrDefaultAsync();
            res1.WallpaperID = WallpaperID;
            await _ctx.SaveChangesAsync();
            return RedirectToAction("RetBack");
        }
        public async Task<IActionResult> BuyGame(string GameName)
        {
            var currdate = DateTime.Now;
            var res = await _ctx.GemesPrices.Where(c => c.GameEndDate > currdate && c.GameStartDate <= currdate)
                .Join(_ctx.Games,
                    gp => gp.GameID,
                    g => g.GameID,
                    (gp, g) => new {
                        GameID = g.GameID,
                        GamePrice = gp.GamePrice,
                        GameCover = g.GameCover,
                        GameName = g.GameName
                    }).Where(c => c.GameName == GameName)
                .Join(_ctx.GameGenre,
                    g => g.GameID,
                    gg => gg.GameID,
                    (g, gg) => new {
                        GamePrice = g.GamePrice,
                        GameCover = g.GameCover,
                        GameName = g.GameName,
                        GenreID = gg.GenreID
                    })
                .Join(_ctx.Genres,
                    g => g.GenreID,
                    gs => gs.GenreID,
                    (g, gs) => new {
                        GamePrice = g.GamePrice,
                        GameCover = g.GameCover,
                        GameName = g.GameName,
                        GenreName = gs.GenreName
                    }).ToListAsync();
            int GamePrice = 0;
            string GameCover = "";
            string _GameName = "";
            string GameGenre = "";
            foreach (var a in res)
                if (!_GameName.Contains(a.GameName)) {
                    GamePrice = a.GamePrice;
                    _GameName = a.GameName;
                    GameCover = Convert.ToBase64String(a.GameCover);
                    GameGenre = a.GenreName;
                }
                else GameGenre += $", {a.GenreName}";
            return View(new GetBuyGame(GameName,GameCover,GameGenre,GamePrice));
        }
        public IActionResult BuyGIW(string GIW, string ItemName)
        {
            var cCash = int.Parse(HttpContext.Request.Cookies["ClientCash"]);
            var ACT = GetACT(GIW, ItemName);
            return View(new BuyForm(GIW, cCash, ACT.Result, ItemName, YoN(GIW, ItemName).Result));
        }
        private async Task<int> GetACT(string GIW, string itemName)
        {
            var currdate = DateTime.UtcNow;
            switch (GIW) {
                case "I":
                    return await _ctx.Inventory.Where(c => c.InventoryName == itemName)
                        .Join(_ctx.InventoryIPrices,
                            g => g.InventoryID,
                            gps => gps.InventoryID,
                            (g, gps) => new {
                                InventoryIPrice = gps.InventoryIPrice,
                                ISD = gps.InventoryStartDate,
                                IED = gps.InventoryEndDate
                            }).Where(c => c.ISD < currdate && c.IED > currdate)
                            .Select(c => c.InventoryIPrice).FirstOrDefaultAsync();
                case "W":
                    return await _ctx.Wallpapers.Where(c => c.WallpaperName == itemName)
                    .Join(_ctx.WallpaperPrice,
                        g => g.WallpaperID,
                        gps => gps.WallpaperID,
                        (g, gps) => new {
                            WallpaperPrice = gps.WallpaperPrice1,
                            WSD = gps.WallpaperStartDate,
                            WED = gps.WallpaperEndDate
                        }).Where(c => c.WSD < currdate && c.WED > currdate)
                        .Select(c => c.WallpaperPrice).FirstOrDefaultAsync();
                default:
                    return await _ctx.Games.Where(c => c.GameName == itemName)
                    .Join(_ctx.GemesPrices,
                        g => g.GameID,
                        gps => gps.GameID,
                        (g, gps) => new {
                            GamePrice = gps.GamePrice,
                            GSD = gps.GameStartDate,
                            GED = gps.GameEndDate
                        }).Where(c => c.GSD < currdate && c.GED > currdate)
                        .Select(c => c.GamePrice).FirstOrDefaultAsync();
            }
        }
        private async Task<bool> YoN(string GIW, string itemName)
        {
            var cLogin = HttpContext.Request.Cookies["ClientLogin"];
            var res1 = await _ctx.Client.Where(c=>c.ClientLogin == cLogin).FirstOrDefaultAsync();
            switch (GIW) {
                case "I":
                    return await _ctx.ClientsInventory.Where(c => c.ClientID == res1.ClientID)
                        .Join(_ctx.Inventory,
                            cg => cg.InventoryID,
                            g => g.InventoryID,
                            (cg, g) => new {
                                Inventoryname = g.InventoryName
                            }).Where(c => c.Inventoryname == itemName).CountAsync() > 0;
                case "W":
                    return await _ctx.ClientsWallpapers.Where(c => c.ClientID == res1.ClientID)
                        .Join(_ctx.Wallpapers,
                            cg => cg.WallpaperID,
                            g => g.WallpaperID,
                            (cg, g) => new {
                                Wallpapername = g.WallpaperName
                            }).Where(c => c.Wallpapername == itemName).CountAsync() > 0;
                default:
                    return await _ctx.ClientsGames.Where(c => c.ClientID == res1.ClientID)
                        .Join(_ctx.Games,
                            cg => cg.GameID,
                            g => g.GameID,
                            (cg, g) => new {
                                Gamename = g.GameName
                            }).Where(c => c.Gamename == itemName).CountAsync() > 0;
            }
        }
        public void reloadAcc(string cLogin,int? cCash)
        {
            Response.Cookies.Append("ClientLogin", $"{cLogin}");
            Response.Cookies.Append("ClientCash", $"{cCash}");
        }
        [Obsolete]
        public async Task<IActionResult> BuyGIWActive(string GIW, string Name, int ACT)
        {
            var currdate = DateTime.Now;
            var cLogin = HttpContext.Request.Cookies["ClientLogin"];
            var cID = await _ctx.Client.Where(c => c.ClientLogin == cLogin).Select(c => c.ClientID).FirstOrDefaultAsync();
            var _new = await _ctx.Database.ExecuteSqlCommandAsync(
                "insert into Purchases(ClientID,PurchaseDate,PurchaseCount)" +
                "values(@ClientID,@Date,@Count)",
                new SqlParameter("@ClientID", cID),
                new SqlParameter("@Date",currdate),
                new SqlParameter("@Count",ACT));
            switch (GIW) {
                case "I":
                    var res1 = await _ctx.Inventory.Where(c => c.InventoryName == Name).Select(c => c.InventoryID).FirstOrDefaultAsync();
                    var _new2 = await _ctx.Database.ExecuteSqlCommandAsync(
                        "insert into ClientsInventory(InventoryID,ClientID,BuyDate)" +
                        "values(@InvID,@ClientID,@Date)",
                        new SqlParameter("@InvID",res1),
                        new SqlParameter("@ClientID",cID),
                        new SqlParameter("@Date", currdate));
                    break;
                case "W":
                    var res2 = await _ctx.Wallpapers.Where(c => c.WallpaperName == Name).Select(c => c.WallpaperID).FirstOrDefaultAsync();
                    var _new3 = await _ctx.Database.ExecuteSqlCommandAsync(
                        "insert into ClientsWallpapers(WallpaperID,ClientID,BuyDateWal)" +
                        "values(@WalID,@ClientID,@Date)",
                        new SqlParameter("@WalID", res2),
                        new SqlParameter("@ClientID", cID),
                        new SqlParameter("@Date", currdate));
                    break;
                default:
                    var res5 = await _ctx.Games.Where(c => c.GameName == Name).Select(c => c.GameID).FirstOrDefaultAsync();
                    var _new4 = await _ctx.Database.ExecuteSqlCommandAsync(
                        "insert into ClientsGames(GameID,ClientID,GamePurchaseDate)" +
                        "values(@GameID,@ClientID,@Date)",
                        new SqlParameter("@GameID", res5),
                        new SqlParameter("@ClientID", cID),
                        new SqlParameter("@Date", currdate));
                    var res4 = await _ctx.Games.Where(c => c.GameName == Name).FirstOrDefaultAsync();
                    res4.CountGameBay++;
                    break;
            }
            await _ctx.SaveChangesAsync();
            var res3 = await _ctx.Client.Where(c => c.ClientLogin == cLogin).FirstOrDefaultAsync();
            reloadAcc(res3.ClientLogin,res3.cash);
            return RedirectToAction("Store");
        }
        public IActionResult Manager()
        {
            return View();
        }
        [HttpPost]
        public JsonResult GetSearchingData(string SearchWho, string SearchBy, string SearchValue)
        {
            switch (SearchWho) {
                case "WallpaperPrice":
                    return Json(getTableWallpaperPrice(SearchBy, SearchValue));
                case "Wallpapers":
                    return Json(getTableWallpapers(SearchBy, SearchValue));
                case "Games":
                    return Json(getTableGames(SearchBy, SearchValue));
                case "InventoryIPrices":
                    return Json(getTableInventoryIPrices(SearchBy, SearchValue));
                case "Inventory":
                    return Json(getTableInventory(SearchBy, SearchValue));
                case "GemesPrices":
                    return Json(getTableGemesPrices(SearchBy, SearchValue));
                case "GameGenre":
                    return Json(getTableGameGenre(SearchBy, SearchValue));
                case "Genres":
                    return Json(getTableGeres(SearchBy, SearchValue));
                default:
                    return Json(getTableStaff(SearchBy, SearchValue));
            }
            
        }
        private List<Staff> getTableStaff(string SearchBy, string SearchValue)
        {
            if (SearchValue == null) return _ctx.Staff.ToList();
            switch (SearchBy) {
                default:
                    return _ctx.Staff.Where(c => c.StaffName == SearchValue).ToList();
                case "StaffLogin":
                    return _ctx.Staff.Where(c => c.StaffLogin == SearchValue).ToList();
            }
        }
        private List<listGameGenre> getTableGameGenre(string SearchBy, string SearchValue)
        {
            if (SearchValue == null)
                return _ctx.GameGenre
                    .Join(_ctx.Games,
                    gg => gg.GameID,
                    g => g.GameID,
                    (gg, g) => new {
                        GenreID = gg.GenreID,
                        GameName = g.GameName
                    }).Join(_ctx.Genres,
                    g => g.GenreID,
                    gg => gg.GenreID,
                    (g, gg) => new {
                        GameName = g.GameName,
                        Genre = gg.GenreName
                    }).Select(c=>new listGameGenre(c.GameName,c.Genre))
                    .ToList();
            switch (SearchBy) {
                default:
                    return _ctx.GameGenre
                        .Join(_ctx.Games,
                        gg => gg.GameID,
                        g => g.GameID,
                        (gg, g) => new {
                            GenreID = gg.GenreID,
                            GameName = g.GameName
                        }).Join(_ctx.Genres,
                        g => g.GenreID,
                        gg => gg.GenreID,
                        (g, gg) => new {
                            GameName = g.GameName,
                            Genre = gg.GenreName
                        }).Where(c => c.GameName == SearchValue)
                        .Select(c => new listGameGenre(c.GameName, c.Genre))
                        .ToList();
                case "Genre":
                    return _ctx.GameGenre
                        .Join(_ctx.Games,
                        gg => gg.GameID,
                        g => g.GameID,
                        (gg, g) => new {
                            GenreID = gg.GenreID,
                            GameName = g.GameName
                        }).Join(_ctx.Genres,
                        g => g.GenreID,
                        gg => gg.GenreID,
                        (g, gg) => new {
                            GameName = g.GameName,
                            Genre = gg.GenreName
                        }).Where(c => c.Genre == SearchValue)
                        .Select(c => new listGameGenre(c.GameName, c.Genre))
                        .ToList();
            }
        }
        private List<listGemesPrices> getTableGemesPrices(string SearchBy, string SearchValue)
        {
            if (SearchValue == null)
                return _ctx.GemesPrices
                    .Join(_ctx.Games,
                        gp=>gp.GameID,
                        g=>g.GameID,
                        (gp, g) => new {
                            GameName = g.GameName,
                            sDate = gp.GameStartDate,
                            eDate = gp.GameEndDate,
                            Price = gp.GamePrice
                        })
                    .Select(c=>new listGemesPrices(c.GameName, c.sDate, c.eDate,c.Price))
                    .ToList();
            switch (SearchBy) {
                default:
                    return _ctx.GemesPrices
                    .Join(_ctx.Games,
                        gp => gp.GameID,
                        g => g.GameID,
                        (gp, g) => new {
                            GameName = g.GameName,
                            sDate = gp.GameStartDate,
                            eDate = gp.GameEndDate,
                            Price = gp.GamePrice
                        })
                    .Where(c=>c.GameName == SearchValue)
                    .Select(c => new listGemesPrices(c.GameName, c.sDate, c.eDate, c.Price))
                    .ToList();
                case "GameStartDate":
                    if (!DateTime.TryParse(SearchValue, out DateTime dateTime)) dateTime = DateTime.UtcNow;
                    return _ctx.GemesPrices
                    .Join(_ctx.Games,
                        gp => gp.GameID,
                        g => g.GameID,
                        (gp, g) => new {
                            GameName = g.GameName,
                            sDate = gp.GameStartDate,
                            eDate = gp.GameEndDate,
                            Price = gp.GamePrice
                        })
                    .Where(c => c.sDate == dateTime)
                    .Select(c => new listGemesPrices(c.GameName, c.sDate, c.eDate, c.Price))
                    .ToList();
                case "GameEndDate":
                    if (!DateTime.TryParse(SearchValue, out DateTime dateTime1)) dateTime1 = DateTime.UtcNow;
                    return _ctx.GemesPrices
                    .Join(_ctx.Games,
                        gp => gp.GameID,
                        g => g.GameID,
                        (gp, g) => new {
                            GameName = g.GameName,
                            sDate = gp.GameStartDate,
                            eDate = gp.GameEndDate,
                            Price = gp.GamePrice
                        })
                    .Where(c => c.eDate == dateTime1)
                    .Select(c => new listGemesPrices(c.GameName, c.sDate, c.eDate, c.Price))
                    .ToList();
                case "Price":
                    if (!int.TryParse(SearchValue, out int number)) number = 0;
                    return _ctx.GemesPrices
                    .Join(_ctx.Games,
                        gp => gp.GameID,
                        g => g.GameID,
                        (gp, g) => new {
                            GameName = g.GameName,
                            sDate = gp.GameStartDate,
                            eDate = gp.GameEndDate,
                            Price = gp.GamePrice
                        })
                    .Where(c => c.Price == number)
                    .Select(c => new listGemesPrices(c.GameName, c.sDate, c.eDate, c.Price))
                    .ToList();
            }
        }
        private List<Genres> getTableGeres(string SearchBy, string SearchValue)
        {
            if (SearchValue == null) return _ctx.Genres.ToList();
            return _ctx.Genres.Where(c => c.GenreName == SearchValue).ToList();
        }
        private List<listInventory> getTableInventory(string SearchBy, string SearchValue)
        {
            if (SearchValue == null)
                return _ctx.Inventory
                    .Join(_ctx.Games,
                        i => i.GameID,
                        g => g.GameID,
                        (i, g) => new {
                            InventoryName = i.InventoryName,
                            GameName = g.GameName
                        })
                    .Select(c => new listInventory(c.InventoryName, c.GameName))
                    .ToList();
            switch (SearchBy) {
                default:
                    return _ctx.Inventory
                    .Join(_ctx.Games,
                        i => i.GameID,
                        g => g.GameID,
                        (i, g) => new {
                            InventoryName = i.InventoryName,
                            GameName = g.GameName
                        })
                    .Where(c=>c.InventoryName == SearchValue)
                    .Select(c => new listInventory(c.InventoryName, c.GameName))
                    .ToList();
                case "GameName":
                    return _ctx.Inventory
                    .Join(_ctx.Games,
                        i => i.GameID,
                        g => g.GameID,
                        (i, g) => new {
                            InventoryName = i.InventoryName,
                            GameName = g.GameName
                        })
                    .Where(c => c.GameName == SearchValue)
                    .Select(c => new listInventory(c.InventoryName, c.GameName))
                    .ToList();
            }
        }
        private List<listInventoryIPrices> getTableInventoryIPrices(string SearchBy, string SearchValue)
        {
            if (SearchValue == null)
                return _ctx.InventoryIPrices
                    .Join(_ctx.Inventory,
                        ip => ip.InventoryID,
                        i => i.InventoryID,
                        (ip, i) => new {
                            InventoryName = i.InventoryName,
                            sDate = ip.InventoryStartDate,
                            eDate = ip.InventoryEndDate,
                            Price = ip.InventoryIPrice
                        })
                    .Select(c => new listInventoryIPrices(c.InventoryName, c.sDate, c.eDate, c.Price))
                    .ToList();
            switch (SearchBy) {
                default:
                    return _ctx.InventoryIPrices
                    .Join(_ctx.Inventory,
                        ip => ip.InventoryID,
                        i => i.InventoryID,
                        (ip, i) => new {
                            InventoryName = i.InventoryName,
                            sDate = ip.InventoryStartDate,
                            eDate = ip.InventoryEndDate,
                            Price = ip.InventoryIPrice
                        })
                    .Where(c=>c.InventoryName == SearchValue)
                    .Select(c => new listInventoryIPrices(c.InventoryName, c.sDate, c.eDate, c.Price))
                    .ToList();
                case "InventoryStartDate":
                    if (!DateTime.TryParse(SearchValue, out DateTime dateTime)) dateTime = DateTime.UtcNow;
                    return _ctx.InventoryIPrices
                    .Join(_ctx.Inventory,
                        ip => ip.InventoryID,
                        i => i.InventoryID,
                        (ip, i) => new {
                            InventoryName = i.InventoryName,
                            sDate = ip.InventoryStartDate,
                            eDate = ip.InventoryEndDate,
                            Price = ip.InventoryIPrice
                        })
                    .Where(c => c.sDate == dateTime)
                    .Select(c => new listInventoryIPrices(c.InventoryName, c.sDate, c.eDate, c.Price))
                    .ToList();
                case "InventoryEndDate":
                    if (!DateTime.TryParse(SearchValue, out DateTime dateTime1)) dateTime1 = DateTime.UtcNow;
                    return _ctx.InventoryIPrices
                    .Join(_ctx.Inventory,
                        ip => ip.InventoryID,
                        i => i.InventoryID,
                        (ip, i) => new {
                            InventoryName = i.InventoryName,
                            sDate = ip.InventoryStartDate,
                            eDate = ip.InventoryEndDate,
                            Price = ip.InventoryIPrice
                        })
                    .Where(c => c.eDate == dateTime1)
                    .Select(c => new listInventoryIPrices(c.InventoryName, c.sDate, c.eDate, c.Price))
                    .ToList();
                case "Price":
                    if (!int.TryParse(SearchValue, out int number)) number = 0;
                    return _ctx.InventoryIPrices
                    .Join(_ctx.Inventory,
                        ip => ip.InventoryID,
                        i => i.InventoryID,
                        (ip, i) => new {
                            InventoryName = i.InventoryName,
                            sDate = ip.InventoryStartDate,
                            eDate = ip.InventoryEndDate,
                            Price = ip.InventoryIPrice
                        })
                    .Where(c => c.Price == number)
                    .Select(c => new listInventoryIPrices(c.InventoryName, c.sDate, c.eDate, c.Price))
                    .ToList();
            }
        }
        private List<listGames> getTableGames(string SearchBy, string SearchValue)
        {
            if (SearchValue == null)
                return _ctx.Games
                    .Select(c => new listGames(c.GameName, c.GameCover, c.DeveloperName, c.CountGameBay, c.Prt))
                    .ToList();
            switch (SearchBy) {
                default:
                    return _ctx.Games
                        .Where(c=>c.GameName == SearchValue)
                        .Select(c => new listGames(c.GameName, c.GameCover, c.DeveloperName, c.CountGameBay, c.Prt))
                        .ToList();
                case "DeveloperName":
                    return _ctx.Games
                        .Where(c => c.DeveloperName == SearchValue)
                        .Select(c => new listGames(c.GameName, c.GameCover, c.DeveloperName, c.CountGameBay, c.Prt))
                        .ToList();
                case "CountGameBay":
                    if (!int.TryParse(SearchValue, out int number)) number = 0;
                    return _ctx.Games
                        .Where(c => c.CountGameBay == number)
                        .Select(c => new listGames(c.GameName, c.GameCover, c.DeveloperName, c.CountGameBay, c.Prt))
                        .ToList();
                case "Prt":
                    if (!int.TryParse(SearchValue, out int number1)) number1 = 0;
                    return _ctx.Games
                        .Where(c => c.Prt == number1)
                        .Select(c => new listGames(c.GameName, c.GameCover, c.DeveloperName, c.CountGameBay, c.Prt))
                        .ToList();
            }
        }
        private List<listWallpapers> getTableWallpapers(string SearchBy, string SearchValue)
        {
            if (SearchValue == null) 
                return _ctx.Wallpapers
                    .Select(c => new listWallpapers(c.WallpaperName, c.WallpaperPhoto))
                    .ToList();
            return _ctx.Wallpapers
                .Where(c=>c.WallpaperName == SearchValue)
                .Select(c => new listWallpapers(c.WallpaperName, c.WallpaperPhoto))
                .ToList();
        }
        private List<listWallpaperPrice> getTableWallpaperPrice(string SearchBy, string SearchValue)
        {
            if (SearchValue == null)
                return _ctx.WallpaperPrice
                    .Join(_ctx.Wallpapers,
                        ip => ip.WallpaperID,
                        i => i.WallpaperID,
                        (ip, i) => new
                        {
                            WallpapersName = i.WallpaperName,
                            sDate = ip.WallpaperStartDate,
                            eDate = ip.WallpaperEndDate,
                            Price = ip.WallpaperPrice1
                        })
                    .Select(c => new listWallpaperPrice(c.WallpapersName, c.sDate, c.eDate, c.Price))
                    .ToList();
            switch (SearchBy) {
                default:
                    return _ctx.WallpaperPrice
                    .Join(_ctx.Wallpapers,
                        ip => ip.WallpaperID,
                        i => i.WallpaperID,
                        (ip, i) => new
                        {
                            WallpapersName = i.WallpaperName,
                            sDate = ip.WallpaperStartDate,
                            eDate = ip.WallpaperEndDate,
                            Price = ip.WallpaperPrice1
                        })
                    .Where(c => c.WallpapersName == SearchValue)
                    .Select(c => new listWallpaperPrice(c.WallpapersName, c.sDate, c.eDate, c.Price))
                    .ToList();
                case "WallpaperStartDate":
                    if (!DateTime.TryParse(SearchValue, out DateTime dateTime)) dateTime = DateTime.UtcNow;
                    return _ctx.WallpaperPrice
                    .Join(_ctx.Wallpapers,
                        ip => ip.WallpaperID,
                        i => i.WallpaperID,
                        (ip, i) => new
                        {
                            WallpapersName = i.WallpaperName,
                            sDate = ip.WallpaperStartDate,
                            eDate = ip.WallpaperEndDate,
                            Price = ip.WallpaperPrice1
                        })
                    .Where(c => c.sDate == dateTime)
                    .Select(c => new listWallpaperPrice(c.WallpapersName, c.sDate, c.eDate, c.Price))
                    .ToList();
                case "WallpaperEndDate":
                    if (!DateTime.TryParse(SearchValue, out DateTime dateTime1)) dateTime1 = DateTime.UtcNow;
                    return _ctx.WallpaperPrice
                    .Join(_ctx.Wallpapers,
                        ip => ip.WallpaperID,
                        i => i.WallpaperID,
                        (ip, i) => new
                        {
                            WallpapersName = i.WallpaperName,
                            sDate = ip.WallpaperStartDate,
                            eDate = ip.WallpaperEndDate,
                            Price = ip.WallpaperPrice1
                        })
                    .Where(c => c.eDate == dateTime1)
                    .Select(c => new listWallpaperPrice(c.WallpapersName, c.sDate, c.eDate, c.Price))
                    .ToList();
                case "Price":
                    if (!int.TryParse(SearchValue, out int number)) number = 0;
                    return _ctx.WallpaperPrice
                    .Join(_ctx.Wallpapers,
                        ip => ip.WallpaperID,
                        i => i.WallpaperID,
                        (ip, i) => new
                        {
                            WallpapersName = i.WallpaperName,
                            sDate = ip.WallpaperStartDate,
                            eDate = ip.WallpaperEndDate,
                            Price = ip.WallpaperPrice1
                        })
                    .Where(c => c.Price == number)
                    .Select(c => new listWallpaperPrice(c.WallpapersName, c.sDate, c.eDate, c.Price))
                    .ToList();
            }
        }
        public IActionResult ManagerEdit()
        {
            return View();
        }
        [Obsolete]
        public JsonResult managerEditFunc(string type, string SearchWho, string SearchBy, string SearchValue, string EditValue)
        {
            return Json(managerEditAdd(SearchWho,EditValue));
        }
        public JsonResult managerEditUpdate(string idtable, string EditValue)
        {
            return Json(managerEditUpdateFunc(idtable, EditValue));
        }
        private string managerEditUpdateFunc(string idtable, string EditValue)
        {
            var a = EditValue.Split(':');
            var b = idtable.Split(':');
            if(!int.TryParse(b[0],out int id)) return "Ошибка, неверный формат id";
            switch (b[1]) {
                default:
                    var res = _ctx.Staff.Where(c => c.StaffID == id).FirstOrDefault();
                    if (res == null) return "Ошибка, такой записи в таблице нет";
                    if (a[0] != "") res.StaffName = a[0];
                    if (a[1] != "") res.StaffLogin = a[1];
                    try {
                        _ctx.SaveChanges();
                        return "true";
                    }
                    catch (Exception ex) { return ex.ToString(); }
                case "GemesPrices":
                    var res1 = _ctx.GemesPrices.Where(c => c.BuyGameID == id).FirstOrDefault();
                    if (res1 == null) return "Ошибка, такой записи в таблице нет";
                    if (a[0] != "") res1.GameID = _ctx.Games.Where(c=>c.GameName == a[0]).Select(c => c.GameID).FirstOrDefault();
                    if (a[1] != "") {
                        if (!DateTime.TryParse(a[1], out DateTime date1)) return "Ошибка, неверный формат даты";
                        res1.GameStartDate = date1;
                    }
                    if (a[2] != "") {
                        if (!DateTime.TryParse(a[2], out DateTime date1)) return "Ошибка, неверный формат даты";
                        res1.GameEndDate = date1;
                    }
                    if (a[3] != "") {
                        if (!int.TryParse(a[3], out int number)) return "Ошибка, нвеверный формат числа";
                        res1.GamePrice = number;
                    }
                    try {
                        _ctx.SaveChanges();
                        return "true";
                    }
                    catch (Exception ex) { return ex.ToString(); }
                case "InventoryIPrices":
                    var res2 = _ctx.InventoryIPrices.Where(c => c.BayInventoryID == id).FirstOrDefault();
                    if (res2 == null) return "Ошибка, такой записи в таблице нет";
                    if (a[0] != "") res2.InventoryID = _ctx.Inventory.Where(c => c.InventoryName == a[0]).Select(c => c.InventoryID).FirstOrDefault();
                    if (a[1] != "") {
                        if (!DateTime.TryParse(a[1], out DateTime date1)) return "Ошибка, неверный формат даты";
                        res2.InventoryStartDate = date1;
                    }
                    if (a[2] != "") {
                        if (!DateTime.TryParse(a[2], out DateTime date1)) return "Ошибка, неверный формат даты";
                        res2.InventoryEndDate = date1;
                    }
                    if (a[3] != "") {
                        if (!int.TryParse(a[3], out int number)) return "Ошибка, нвеверный формат числа";
                        res2.InventoryIPrice = number;
                    }
                    try {
                        _ctx.SaveChanges();
                        return "true";
                    }
                    catch (Exception ex) { return ex.ToString(); }
                case "WallpaperPrice":
                    var res3 = _ctx.WallpaperPrice.Where(c => c.BuyWallpaperID == id).FirstOrDefault();
                    if (res3 == null) return "Ошибка, такой записи в таблице нет";
                    if (a[0] != "") res3.WallpaperID = _ctx.Wallpapers.Where(c => c.WallpaperName == a[0]).Select(c => c.WallpaperID).FirstOrDefault();
                    if (a[1] != "") {
                        if (!DateTime.TryParse(a[1], out DateTime date1)) return "Ошибка, неверный формат даты";
                        res3.WallpaperStartDate = date1;
                    }
                    if (a[2] != "") {
                        if (!DateTime.TryParse(a[2], out DateTime date1)) return "Ошибка, неверный формат даты";
                        res3.WallpaperEndDate = date1;
                    }
                    if (a[3] != "") {
                        if (!int.TryParse(a[3], out int number)) return "Ошибка, нвеверный формат числа";
                        res3.WallpaperPrice1 = number;
                    }
                    try {
                        _ctx.SaveChanges();
                        return "true";
                    }
                    catch (Exception ex) { return ex.ToString(); }
                case "Genres":
                    var res4 = _ctx.Genres.Where(c => c.GenreID == id).FirstOrDefault();
                    if (res4 == null) return "Ошибка, такой записи в таблице нет";
                    if (a[0] != "") res4.GenreName = a[0];
                    try {
                        _ctx.SaveChanges();
                        return "true";
                    }
                    catch (Exception ex) { return ex.ToString(); }
                case "Inventory":
                    var res5 = _ctx.Inventory.Where(c => c.InventoryID == id).FirstOrDefault();
                    if (res5 == null) return "Ошибка, такой записи в таблице нет";
                    if (a[0] != "") res5.InventoryName = a[0];
                    if (a[1] != "") res5.GameID = _ctx.Games.Where(c=>c.GameName==a[1]).Select(c=>c.GameID).FirstOrDefault();
                    try {
                        _ctx.SaveChanges();
                        return "true";
                    }
                    catch (Exception ex) { return ex.ToString(); }
            }
        }
        public JsonResult eventRemoveJson(string idtable)
        {
            var a = idtable.Split(':');
            switch (a[1]) {
                default:
                    if(!int.TryParse(a[0], out int number)) return Json("Ошибка, неверный id объекта");
                    try {
                        var res1 = _ctx.Staff.Where(c => c.StaffID == number).FirstOrDefault();
                        _ctx.Staff.Remove(res1);
                        _ctx.SaveChanges();
                    }
                    catch { return Json("Ошибка удаления из таблицы"); }
                    return Json("true");
                case "GameGenre":
                    if (!int.TryParse(a[0], out int number1)) return Json("Ошибка, неверный id объекта");
                    try {
                        var res1 = _ctx.GameGenre.Where(c => c.GGID == number1).FirstOrDefault();
                        _ctx.GameGenre.Remove(res1);
                        _ctx.SaveChanges();
                    }
                    catch { return Json("Ошибка удаления из таблицы"); }
                    return Json("true");
                case "GemesPrices":
                    if (!int.TryParse(a[0], out int number2)) return Json("Ошибка, неверный id объекта");
                    try {
                        var res2 = _ctx.GemesPrices.Where(c => c.BuyGameID == number2).FirstOrDefault();
                        _ctx.GemesPrices.Remove(res2);
                        _ctx.SaveChanges();
                    }
                    catch { return Json("Ошибка удаления из таблицы"); }
                    return Json("true");
                case "Genres":
                    if (!int.TryParse(a[0], out int number3)) return Json("Ошибка, неверный id объекта");
                    try {
                        var res3 = _ctx.Genres.Where(c => c.GenreID == number3).FirstOrDefault();
                        _ctx.Genres.Remove(res3);
                        _ctx.SaveChanges();
                    }
                    catch { return Json("Ошибка удаления из таблицы"); }
                    return Json("true");
                case "Inventory":
                    if (!int.TryParse(a[0], out int number4)) return Json("Ошибка, неверный id объекта");
                    try {
                        var res3 = _ctx.Inventory.Where(c => c.InventoryID == number4).FirstOrDefault();
                        _ctx.Inventory.Remove(res3);
                        _ctx.SaveChanges();
                    }
                    catch { return Json("Ошибка удаления из таблицы"); }
                    return Json("true");
                case "InventoryIPrices":
                    if (!int.TryParse(a[0], out int number5)) return Json("Ошибка, неверный id объекта");
                    try {
                        var res3 = _ctx.InventoryIPrices.Where(c => c.BayInventoryID == number5).FirstOrDefault();
                        _ctx.InventoryIPrices.Remove(res3);
                        _ctx.SaveChanges();
                    }
                    catch { return Json("Ошибка удаления из таблицы"); }
                    return Json("true");
                case "WallpaperPrice":
                    if (!int.TryParse(a[0], out int number6)) return Json("Ошибка, неверный id объекта");
                    try {
                        var res3 = _ctx.WallpaperPrice.Where(c => c.BuyWallpaperID == number6).FirstOrDefault();
                        _ctx.WallpaperPrice.Remove(res3);
                        _ctx.SaveChanges();
                    }
                    catch { return Json("Ошибка удаления из таблицы"); }
                    return Json("true");
            }
        }
        [Obsolete]
        private string managerEditAdd(string SearchWho, string EditValue)
        {
            var a = EditValue.Split(':');
            for (int i = 0; i < a.Length; i++)
                if (a[i] == "") return "Ошибка, заполните все поля";
            switch (SearchWho) {
                default:
                    try {
                        var res12 = _ctx.Client.Where(c => c.ClientLogin == a[1]).FirstOrDefault();
                        var res3 = _ctx.Staff.Where(c => c.StaffLogin == a[1]).FirstOrDefault();
                        if (res3 != null||res12 != null) return "Ошибка, такой аккаунт уже существует";
                        int res1 = _ctx.Database.ExecuteSqlCommand("insert into Staff" +
                            "(StaffName,StaffLogin,StaffPasword,StaffType)" +
                            "values(@res0,@res1,@Pass,@Type)",
                            new SqlParameter("@res0",a[0]),
                            new SqlParameter("@res1",a[1]),
                            new SqlParameter("@Pass",ComputeSha256Hash("123123")),
                            new SqlParameter("@Type","M"));
                        return "true";
                    }
                    catch { return "false"; }
                case "GameGenre":
                    try {
                        var game1 = _ctx.Games.Where(c=>c.GameName==a[0]).FirstOrDefault();
                        var genre1 = _ctx.Genres.Where(c=>c.GenreName==a[1]).FirstOrDefault();
                        if (game1 == null || genre1 == null) return "Ошибка, повторите ввод";
                        if (_ctx.GameGenre.Where(c=>c.GenreID==genre1.GenreID&&c.GameID==game1.GameID).Count()>0)
                            return "Ошибка, такая запись существует";
                        int res2 = _ctx.Database.ExecuteSqlCommand("insert into GameGenre" +
                            "(GameID,GenreID)" +
                            "values(@res1,@res2)",
                            new SqlParameter("@res1",game1.GameID),
                            new SqlParameter("@res2",genre1.GenreID));
                        return "true";
                    }
                    catch (Exception ex){ return ex.ToString(); }
                case "GemesPrices":
                    try {
                        var res4 = _ctx.Games.Where(c => c.GameName == a[0]).FirstOrDefault();
                        if (res4 == null) return "Ошибка, такой игры нет";
                        if (!DateTime.TryParse(a[1], out DateTime date1)) return "Ошибка, неверный формат даты";
                        if (!DateTime.TryParse(a[2], out DateTime date2)) return "Ошибка, неверный формат даты";
                        if (!int.TryParse(a[3], out int number1)) return "Ошибка, неверный формат числа";
                        if (DateTime.Now >= date1 && DateTime.Now <= date2) {
                            var delold = _ctx.GemesPrices.Where(c => c.GameID == res4.GameID).ToList();
                            delold.ForEach(c => _ctx.GemesPrices.Remove(c));
                            _ctx.SaveChanges();
                        }
                        int res5 = _ctx.Database.ExecuteSqlCommand("insert into GemesPrices" +
                            "(GameID,GameStartDate,GameEndDate,GamePrice)" +
                            "values(@res1,@res2,@res3,@res4)",
                            new SqlParameter("@res1",res4.GameID),
                            new SqlParameter("@res2",date1),
                            new SqlParameter("@res3",date2),
                            new SqlParameter("@res4",number1));
                        return "true";
                    }
                    catch (Exception ex){ return ex.ToString(); }
                case "Genres":
                    try {
                        var res6 = _ctx.Genres.Where(c => c.GenreName == a[0]).FirstOrDefault();
                        if (res6 != null) return "Ошибка, такой жанр уже есть";
                        int res5 = _ctx.Database.ExecuteSqlCommand("insert into Genres" +
                            "(GenreName)" +
                            "values(@res1)",
                            new SqlParameter("@res1", a[0]));
                        return "true";
                    }
                    catch (Exception ex) { return ex.ToString(); }
                case "Inventory":
                    try {
                        var res7 = _ctx.Games.Where(c => c.GameName == a[1]).FirstOrDefault();
                        if (res7 == null) return "Ошибка, такой игры нет";
                        int res8 = _ctx.Database.ExecuteSqlCommand("insert into Inventory" +
                            "(InventoryName,GameID)" +
                            "values(@res1,@res2)",
                            new SqlParameter("@res1",a[0]),
                            new SqlParameter("@res2",res7.GameID));
                        return "true";
                    }
                    catch (Exception ex) { return ex.ToString(); }
                case "InventoryIPrices":
                    try {
                        var res9 = _ctx.Inventory.Where(c => c.InventoryName == a[0]).FirstOrDefault();
                        if (res9 == null) return "Ошибка, такого инвентаря нет";
                        if (!DateTime.TryParse(a[1], out DateTime date1)) return "Ошибка, неверный формат даты";
                        if (!DateTime.TryParse(a[2], out DateTime date2)) return "Ошибка, неверный формат даты";
                        if (!int.TryParse(a[3], out int number1)) return "Ошибка, неверный формат числа";
                        if (DateTime.Now>=date1&& DateTime.Now <= date2) {
                            var delold = _ctx.InventoryIPrices.Where(c => c.InventoryID == res9.InventoryID).ToList();
                            delold.ForEach(c => _ctx.InventoryIPrices.Remove(c));
                            _ctx.SaveChanges();
                        }
                        int res10 = _ctx.Database.ExecuteSqlCommand("insert into InventoryIPrices" +
                            "(InventoryID,InventoryStartDate,InventoryEndDate,InventoryIPrice)" +
                            "values(@res1,@res2,@res3,@res4)",
                            new SqlParameter("@res1", res9.InventoryID),
                            new SqlParameter("@res2", date1),
                            new SqlParameter("@res3", date2),
                            new SqlParameter("@res4", number1));
                        return "true";
                    }
                    catch (Exception ex) { return ex.ToString(); }
                case "WallpaperPrice":
                    try {
                        var res11 = _ctx.Wallpapers.Where(c => c.WallpaperName == a[0]).FirstOrDefault();
                        if (res11 == null) return "Ошибка, таких обоев нет";
                        if (!DateTime.TryParse(a[1], out DateTime date1)) return "Ошибка, неверный формат даты";
                        if (!DateTime.TryParse(a[2], out DateTime date2)) return "Ошибка, неверный формат даты";
                        if (!int.TryParse(a[3], out int number1)) return "Ошибка, неверный формат числа";
                        if (DateTime.Now >= date1 && DateTime.Now <= date2) {
                            var delold = _ctx.WallpaperPrice.Where(c => c.WallpaperID == res11.WallpaperID).ToList();
                            delold.ForEach(c => _ctx.WallpaperPrice.Remove(c));
                            _ctx.SaveChanges();
                        }
                        int res12 = _ctx.Database.ExecuteSqlCommand("insert into WallpaperPrice" +
                            "(WallpaperID,WallpaperStartDate,WallpaperEndDate,WallpaperPrice)" +
                            "values(@res1,@res2,@res3,@res4)",
                            new SqlParameter("@res1", res11.WallpaperID),
                            new SqlParameter("@res2", date1),
                            new SqlParameter("@res3", date2),
                            new SqlParameter("@res4", number1));
                        return "true";
                    }
                    catch (Exception ex) { return ex.ToString(); }
            }
        }
        public FileContentResult Export()
        {
            var curD = DateTime.UtcNow;
            var lasD = DateTime.UtcNow.AddDays(-3);
            var res1 = _ctx.Purchases
                .Where(c => c.PurchaseDate > lasD && c.PurchaseDate <= curD)
                .Join(_ctx.Client,
                    p => p.ClientID,
                    c => c.ClientID,
                    (p, c) => new {
                        ClientName = c.ClientName,
                        PurchaseCount = p.PurchaseCount
                    })
                .GroupBy(
                    c => c.ClientName,
                    p => p.PurchaseCount,
                    (key, g) => new {
                        ClientName = key,
                        PurchaseCount = g.Sum()
                    })
                .ToList();
            using (XLWorkbook workbook = new XLWorkbook(XLEventTracking.Disabled)) {
                var worksheet = workbook.Worksheets.Add("Покупки");
                worksheet.Cell("A1").Value = "ФИО";
                worksheet.Cell("B1").Value = "Кол-во потраченных средств";
                worksheet.Row(1).Style.Font.Bold = true;
                for (int i = 0; i < res1.Count(); i++) {
                    worksheet.Cell(i + 2, 1).Value = res1[i].ClientName;
                    worksheet.Cell(i + 2, 2).Value = res1[i].PurchaseCount;
                }
                using (var stream = new MemoryStream()) {
                    workbook.SaveAs(stream);
                    stream.Flush();
                    return new FileContentResult(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") {
                        FileDownloadName = $"purch_{DateTime.UtcNow.ToShortDateString()}.xlsx"
                    };
                }
            }
        }
        public JsonResult getSelectOptions(string key)
        {
            switch (key)
            {
                default:
                    return Json(_ctx.Games.Select(c =>c.GameName).ToList());
                case "WallpapersName":
                    return Json(_ctx.Wallpapers.Select(c=>c.WallpaperName).ToList());
                case "InventoryName":
                    return Json(_ctx.Inventory.Select(c =>c.InventoryName).ToList());
                case "Genre":
                    return Json(_ctx.Genres.Select(c =>c.GenreName).ToList());
            }
        }
        public JsonResult getSearchEdit(string SearchBy,string SearchEditBy,string SearchValue)
        {
            switch (SearchBy)
            {
                default:
                    var res = _ctx.Staff;
                    if (SearchValue != null) {
                        switch (SearchEditBy) {
                            default:
                                res.Where(c => c.StaffName == SearchValue);
                                break;
                            case "StaffLogin":
                                res.Where(c => c.StaffLogin == SearchValue);
                                break;
                        }
                    }
                    return Json(res.ToList());
                case "GameGenre":
                    var res2 = _ctx.GameGenre
                        .Join(_ctx.Games,
                        gg => gg.GameID,
                        g => g.GameID,
                        (gg, g) => new {
                            ggid = gg.GGID,
                            GameName = g.GameName,
                            GenreID = gg.GenreID
                        }).Join(_ctx.Genres,
                        gg => gg.GenreID,
                        g => g.GenreID,
                        (gg, g) => new {
                            ggid = gg.ggid,
                            GameName = gg.GameName,
                            Genre = g.GenreName
                        });
                    if (SearchValue != null) {
                        switch (SearchEditBy) {
                            default:
                                res2 = res2.Where(c => c.GameName == SearchValue);
                                break;
                            case "Genre":
                                res2 = res2.Where(c=>c.Genre == SearchValue);
                                break;
                        }
                    }
                    return Json(res2.ToList());
                case "GemesPrices":
                    var res3 = _ctx.GemesPrices
                        .Join(_ctx.Games,
                        gp=>gp.GameID,
                        g=>g.GameID,
                        (gp,g)=>new {
                            BuyGameID = gp.BuyGameID,
                            Game = g.GameName,
                            GameStartDate = gp.GameStartDate,
                            GameEndDate = gp.GameEndDate,
                            Price = gp.GamePrice
                        });
                    if (SearchValue != null) {
                        switch (SearchEditBy) {
                            default:
                                res3 = res3.Where(c => c.Game == SearchValue);
                                break;
                            case "GameStartDate":
                                if (!DateTime.TryParse(SearchValue, out DateTime date)) break;
                                res3 = res3.Where(c => c.GameStartDate == date);
                                break;
                            case "GameEndDate":
                                if (!DateTime.TryParse(SearchValue, out DateTime date2)) break;
                                res3 = res3.Where(c => c.GameEndDate == date2);
                                break;
                            case "Price":
                                if (!int.TryParse(SearchValue, out int number1)) break;
                                res3 = res3.Where(c => c.Price == number1);
                                break;
                        }
                    }
                    return Json(res3.ToList());
                case "Genres":
                    if (SearchValue != null) return Json(_ctx.Genres.Where(c=>c.GenreName == SearchValue));
                    return Json(_ctx.Genres.ToList());
                case "Inventory":
                    var res4 = _ctx.Inventory
                        .Join(_ctx.Games,
                        i=>i.GameID,
                        g=>g.GameID,
                        (i, g) => new {
                            InventoryID = i.InventoryID,
                            InventoryName = i.InventoryName,
                            GameName = g.GameName
                        });
                    if(SearchValue != null) {
                        switch (SearchEditBy) {
                            default:
                                res4 = res4.Where(c=>c.InventoryName == SearchValue);
                                break;
                            case "GameName":
                                res4 = res4.Where(c=>c.GameName == SearchValue);
                                break;
                        }
                    }
                    return Json(res4.ToList());
                case "InventoryIPrices":
                    var res5 = _ctx.InventoryIPrices
                        .Join(_ctx.Inventory,
                        gp => gp.InventoryID,
                        g => g.InventoryID,
                        (gp, g) => new {
                            BayInventoryID = gp.BayInventoryID,
                            InventoryName = g.InventoryName,
                            InventoryStartDate = gp.InventoryStartDate,
                            InventoryEndDate = gp.InventoryEndDate,
                            Price = gp.InventoryIPrice
                        });
                    if (SearchValue != null) {
                        switch (SearchEditBy) {
                            default:
                                res5 = res5.Where(c => c.InventoryName == SearchValue);
                                break;
                            case "InventoryStartDate":
                                if (!DateTime.TryParse(SearchValue, out DateTime date)) break;
                                res5 = res5.Where(c => c.InventoryStartDate == date);
                                break;
                            case "InventoryEndDate":
                                if (!DateTime.TryParse(SearchValue, out DateTime date2)) break;
                                res5 = res5.Where(c => c.InventoryEndDate == date2);
                                break;
                            case "Price":
                                if (!int.TryParse(SearchValue, out int number1)) break;
                                res5 = res5.Where(c => c.Price == number1);
                                break;
                        }
                    }
                    return Json(res5.ToList());
                case "WallpaperPrice":
                    var res6 = _ctx.WallpaperPrice
                        .Join(_ctx.Wallpapers,
                        gp => gp.WallpaperID,
                        g => g.WallpaperID,
                        (gp, g) => new {
                            BuyWallpaperID = gp.BuyWallpaperID,
                            WallpapersName = g.WallpaperName,
                            WallpaperStartDate = gp.WallpaperStartDate,
                            WallpaperEndDate = gp.WallpaperEndDate,
                            Price = gp.WallpaperPrice1
                        });
                    if (SearchValue != null) {
                        switch (SearchEditBy) {
                            default:
                                res6 = res6.Where(c => c.WallpapersName == SearchValue);
                                break;
                            case "WallpaperStartDate":
                                if (!DateTime.TryParse(SearchValue, out DateTime date)) break;
                                res6 = res6.Where(c => c.WallpaperStartDate == date);
                                break;
                            case "WallpaperEndDate":
                                if (!DateTime.TryParse(SearchValue, out DateTime date2)) break;
                                res6 = res6.Where(c => c.WallpaperEndDate == date2);
                                break;
                            case "Price":
                                if (!int.TryParse(SearchValue, out int number1)) break;
                                res6 = res6.Where(c => c.Price == number1);
                                break;
                        }
                    }
                    return Json(res6.ToList());
            }
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}