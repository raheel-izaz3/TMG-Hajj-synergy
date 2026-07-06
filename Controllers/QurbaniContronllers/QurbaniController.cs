using TMG.Data;
using TMG.Models.FamilyModels;
using TMG.Models.QurbaniModels;
using TMG.Models.ReservationModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using static System.Net.WebRequestMethods;

namespace TMG.Controllers.QurbaniContronllers
{
    public class QurbaniController : Controller
    {
        private readonly TMGDbContext _dbContext;
        private readonly IHttpContextAccessor _http;
        public QurbaniController(TMGDbContext humsDbContext, IHttpContextAccessor http)
        {
            _dbContext = humsDbContext;
            _http = http;
        }

        public IActionResult SearchReceipt() => View();

        [HttpPost]
        public IActionResult SearchReceipt(string FcId)
        {

            FamilyHead fh = _dbContext.FamilyHeads
                .FirstOrDefault(e => e.FcId.Equals("HB-" + FcId));
            return RedirectToAction("QurbaniQueueForm", "PackageReport", new { id = fh.FamilyHeadId });
        }

        [HttpPost]
        public IActionResult Create(int memberId, int qurbaniTypeId)
        {
            Qurbani qurbani = _dbContext.Qurbani.FirstOrDefault(e => e.FamilyHeadId == memberId);

            if (qurbani == null)
            {
                Qurbani q = new Qurbani();
                q.FamilyHeadId = memberId;
                q.Active = true;

                _dbContext.Qurbani.Add(q);
                _dbContext.SaveChanges();
                return Ok(new
                {
                    status = 200,
                    msg = "Qurbani Added",
                    memberId,
                    qurbaniTypeId,
                    q.QurbaniId
                });
            }
            else
            {
                qurbani.Active = true;
                _dbContext.SaveChanges();

                return Ok(new
                {
                    status = 201,
                    msg = "Qurbani Type Changed",
                    memberId,
                    qurbaniTypeId,
                    qurbani.QurbaniId
                });
            }



        }

        [HttpDelete]
        public IActionResult Delete(int memberId, int qurbaniTypeId)
        {
            Qurbani qurbani = _dbContext.Qurbani
                .FirstOrDefault(e => e.FamilyHeadId == memberId);

            if (qurbani != null)
            {
                qurbani.Active = false;

                _dbContext.SaveChanges();
                return Ok(new { status = 200, msg = "Qurbani Deleted", memberId, qurbaniTypeId });
            }
            else
            {
                return Ok(new { status = 201, msg = "No Qurbani Found", memberId, qurbaniTypeId });
            }
        }

        

        public QurbaniBatch AssignQrAndLink(QurbaniBatch batch)
        {

            switch (batch.Name)
            {
                case "Qurbani Batch #1":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/HAD6uAdIBdk8RFBP8bWlaj";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch1.jpg";
                    break;
                case "Qurbani Batch #2":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/G8V5FE78ftM9EcnkiX6hmY";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch2.jpg";
                    break;
                case "Qurbani Batch #3":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/DCy9FOB45SK3QNWE7drI6N";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch3.jpg";
                    break;
                case "Qurbani Batch #4":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/Erm0tx2hPvhIf6Gx9B2lhb";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch4.jpg";
                    break;
                case "Qurbani Batch #5":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/B71HdXkeNw670wafPog4wK";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch5.jpg";
                    break;
                case "Qurbani Batch #6":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/GB8IlVVniHvIKaWJp2aQ0d";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch6.jpg";
                    break;
                case "Qurbani Batch #7":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/JyLvNhYyBxv8PNuXHTYscw";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch7.jpg";
                    break;
                case "Qurbani Batch #8":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/CsB528IvocQ4NKDd25hskU";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch8.jpg";
                    break;
                case "Qurbani Batch #9":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/CCJzD3hPD9pKaLSZX0IKGm";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch9.jpg";
                    break;
                case "Qurbani Batch #10":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/JdrG8hXcASE4DNYDRWDd7u";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch10.jpg";
                    break;
                case "Qurbani Batch #11":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/H2Oo5FLkUSo5noWtfKMVpZ";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch11.jpg";
                    break;
                case "Qurbani Batch #12":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/FyhC2rJc8uME36YGnHb9Az";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch12.jpg";
                    break;
                case "Qurbani Batch #13":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/HUhsdd08Vg86Nb8HNRPAXg";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch13.jpg";
                    break;
                case "Qurbani Batch #14":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/L3qNAeHi5leBcvkKzqnHxA";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch14.jpg";
                    break;
                case "Qurbani Batch #15":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/KyTJBCiiCanI6FUSwrK9c5";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch15.jpg";
                    break;
                case "Qurbani Batch #16":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/IUL4AVZbyZO7OaU1XsujW6";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch16.jpg";
                    break;
                case "Qurbani Batch #17":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/LvRU4QoEuzlA1iFaWTOWbq";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch17.jpg";
                    break;
                case "Qurbani Batch #18":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/Ds9SGNRg5Si7B9Ei3TvjiH";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch18.jpg";
                    break;
                case "Qurbani Batch #19":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/ENHSoTW8ry3F80Qm5ZtFu4";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch19.jpg";
                    break;
                case "Qurbani Batch #20":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/JzoApNfiDa8AkCV6iVAZM1";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch20.jpg";
                    break;
                case "Qurbani Batch #21":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/Ib9vENpgJH8ESu9E7gH8Lo";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch21.jpg";
                    break;
                case "Qurbani Batch #22":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/Gipp61IxKBiGgwGwhGntU4";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch22.jpg";
                    break;
                case "Qurbani Batch #23":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/HqZWlWOUlMEEiaZFLaF336";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch23.jpg";
                    break;
                case "Qurbani Batch #24":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/BrOgRZ0mEBfGoO8SACgHOJ";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch24.jpg";
                    break;
                case "Qurbani Batch #25":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/DJCRNSuiD3oAMw7gPucTzj";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch25.jpg";
                    break;
                case "Qurbani Batch #26":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/E6SaFP1SUg3JlAAEkj11pg";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch26.jpg";
                    break;
                case "Qurbani Batch #27":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/Gz7Lj1amuwSIDIfKBm5DBL";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch27.jpg";
                    break;
                case "Qurbani Batch #28":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/FwMpn60UCYp6eVD6Ah68AD";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch28.jpg";
                    break;
                case "Qurbani Batch #29":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/JroyNCLFzdtAjoVLnF7Hjc";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch29.jpg";
                    break;
                case "Qurbani Batch #30":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/K4vP9FRr3A3FtrsZZzDxIw";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch30.jpg";
                    break;
                case "Qurbani Batch #31":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/K7Po92mI3FZAZJUrEYC2SP";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch31.jpg";
                    break;
                case "Qurbani Batch #32":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/COF5XcMeqge8Hoays5zo92";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch32.jpg";
                    break;
                case "Qurbani Batch #33":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/G2aPlUlHZ8qGJ0HLJgF16w";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch33.jpg";
                    break;
                case "Qurbani Batch #34":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/Hz7sXcHDZF90SD9r24y1CC";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch34.jpg";
                    break;
                case "Qurbani Batch #35":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/LByIrZZe9zIBC5SzmTxZMz";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch35.jpg";
                    break;
                case "Qurbani Batch #36":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/FCk2P9YctSfHCFLC1yImVl";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch36.jpg";
                    break;
                case "Qurbani Batch #37":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/ItRJX6u69iP2kFoj6VEikJ";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch37.jpg";
                    break;
                case "Qurbani Batch #38":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/IFQPBVOVUz7JO5Lh1TbndA";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch38.jpg";
                    break;
                case "Qurbani Batch #39":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/LiI8tETtyUX7UaMG5e7HIl";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch39.jpg";
                    break;
                case "Qurbani Batch #40":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/DJ5sdQIsKpj6YE71Xz0qsl";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch40.jpg";
                    break;
                case "Qurbani Batch #41":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/Kt8G6YCrUzdFHdPYuYSCp0";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch41.jpg";
                    break;
                case "Qurbani Batch #42":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/FwwE1TmUhfO3Ye616vgcmQ";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch42.jpg";
                    break;
                case "Qurbani Batch #43":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/E05JznTlNuFBBUTGCQQ2vs";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch43.jpg";
                    break;
                case "Qurbani Batch #44":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/JwZxcx3mwZkAqUnHrCF9A9";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch44.jpg";
                    break;
                case "Qurbani Batch #45":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/KmusflYQU69JCC4lF5zV1l";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch45.jpg";
                    break;
                case "Qurbani Batch #46":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/JA9VXf1Hn4d62Xz2ZfwjGl";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch46.jpg";
                    break;
                case "Qurbani Batch #47":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/DKPozmsx33OA9vLHnKZ1dx";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch47.jpg";
                    break;
                case "Qurbani Batch #48":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/ExwZdPT9JcP5WzXpOhMUGf";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch48.jpg";
                    break;
                case "Qurbani Batch #49":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/CmnaHe1pXsz6f81iAu13T1";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch49.jpg";
                    break;
                case "Qurbani Batch #50":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/DE1JHDs85m0LwSJtha5ItW";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch50.jpg";
                    break;
                case "Qurbani Batch #51":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/LHDFLnK37y35IP8jbr3k0s";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch51.jpg";
                    break;
                case "Qurbani Batch #52":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/It775FzXBXZGDxbRNCqkzh";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch52.jpg";
                    break;
                case "Qurbani Batch #53":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/Jh4bp88IHyfAY5gkcVK4DP";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch53.jpg";
                    break;
                case "Qurbani Batch #54":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/DBo4raohApFCiarEP44aif";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch54.jpg";
                    break;
                case "Qurbani Batch #55":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/H9UlWW9LRGh2kszHVbCnYy";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch55.jpg";
                    break;
                case "Qurbani Batch #56":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/D5A1UjsYJvX73VJ5brtgSg";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch56.jpg";
                    break;
                case "Qurbani Batch #57":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/Hl1EKIDiJo7BmgfE5OlHEB";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch57.jpg";
                    break;
                case "Qurbani Batch #58":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/EKgD9yrL7Vz82xsIWQhE5S";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch58.jpg";
                    break;
                case "Qurbani Batch #59":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/Jlb2WbWcnW43uU4w4cna4z";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch59.jpg";
                    break;
                case "Qurbani Batch #60":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/D7xN8fIpLGY0jjLzKCVM9b";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch60.jpg";
                    break;
                case "Qurbani Batch #61":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/KWOS7zvHzsfEuNZqJDK7ni";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch61.jpg";
                    break;
                case "Qurbani Batch #62":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/DvugzLAndjP46xoWu4uq8C";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch62.jpg";
                    break;
                case "Qurbani Batch #63":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/JZ7ruyvZJ9yEIkHqWmFaNu";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch63.jpg";
                    break;
                case "Qurbani Batch #64":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/IVp8mHInyXD8gnbTofB5cX";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch64.jpg";
                    break;
                case "Qurbani Batch #65":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/J9knk1T5E1wCuQ10O7UlBJ";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch65.jpg";
                    break;
                case "Qurbani Batch #66":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/BBZGILAxrFF7WJON9oLMf3";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch66.jpg";
                    break;
                case "Qurbani Batch #67":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/DZDILwD2WLo5tvMVL1dB7s";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch67.jpg";
                    break;
                case "Qurbani Batch #68":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/CpJWgcIcIgS7Zp1E6NyiPT";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch68.jpg";
                    break;
                case "Qurbani Batch #69":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/EF1j22KwjVKIlxivEL7g1g";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch69.jpg";
                    break;
                case "Qurbani Batch #70":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/CbUBIYmMVArLWNGQDhb5bx";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch70.jpg";
                    break;
                case "Qurbani Batch #71":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/EZiDtbrbOZf4zK4khhgGk2";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch71.jpg";
                    break;
                case "Qurbani Batch #72":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/EOVmzG9kOmI4idj7cPUXQm";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch72.jpg";
                    break;
                case "Qurbani Batch #73":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/FXrg9pRgQbiGMGNgbvEZyC";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch73.jpg";
                    break;
                case "Qurbani Batch #74":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/GDfoeo8t4rsHUrCxKYM0tJ";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch74.jpg";
                    break;
                case "Qurbani Batch #75":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/II56onzesk9B6qvWgUclV2";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch75.jpg";
                    break;
                case "Qurbani Batch #76":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/KnNMf5Yhbou2FmTGhkXYTD";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch76.jpg";
                    break;
                case "Qurbani Batch #77":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/EWtjoobXsaU8Xja5EOW97w";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch77.jpg";
                    break;
                case "Qurbani Batch #78":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/EDkFG9fjW4jLb4jQddLLeC";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch78.jpg";
                    break;
                case "Qurbani Batch #79":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/JQI52UuFh9x4w8LZPwwHEZ";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch79.jpg";
                    break;
                case "Qurbani Batch #80":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/J5R43sbxwlXBpQjmlRB4B1";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch80.jpg";
                    break;
                case "Qurbani Batch #81":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/CDvGbXqlXeKHV2TYon8Ic6";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch81.jpg";
                    break;
                case "Qurbani Batch #82":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/L2tncwCjBvv774Ak1AebNX";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch82.jpg";
                    break;
                case "Qurbani Batch #83":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/LXCvPHwDFjHHZoLSmln0he";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch83.jpg";
                    break;
                case "Qurbani Batch #84":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/GwkXX7qGqZI16ew9VDwxyM";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch84.jpg";
                    break;
                case "Qurbani Batch #85":
                    batch.WhatsappGroupLink = "https://chat.whatsapp.com/GI4qCUs3WFjHF6UtvRc3ly";
                    batch.WhatsappQrCodePath = "Images/QurbaniBatchQrCodes/QurbaniBatch85.jpg";
                    break;
                default:
                    batch.WhatsappGroupLink = "";
                    batch.WhatsappQrCodePath = "";
                    break;

            }

            return batch;
        }

        [HttpPost]
        public IActionResult ProceedQurbani(int id)
        {
            FamilyHead fh = _dbContext.FamilyHeads.Find(id);

            if (_dbContext.QurbaniQueue
                        .FirstOrDefault(e => e.FamilyHeadId == id) is not null)
            {
                return Ok(new
                {
                    status = 201,
                });
            }

            const int CAPACITY_LIMIT = 50;

            var batches = _dbContext.QurbaniBatches
                    .Where(e => !e.Trashed)
                    .ToList();

            QurbaniBatch batch = null;
            if (batches.Count == 0)
            {
                batch = new QurbaniBatch
                {
                    Name = "Qurbani Batch #1",
                    Capacity = CAPACITY_LIMIT,
                    Completed = false
                };
                batch = AssignQrAndLink(batch);
                _dbContext.QurbaniBatches.Add(batch);
                _dbContext.SaveChanges();

                _dbContext.QurbaniQueue.Add(new QurbaniQueue
                {
                    FamilyHeadId = id,
                    QurbaniBatchId = batch.QurbaniBatchId,
                    StartTime = DateTime.Now
                });

                _dbContext.SaveChanges();
            }
            else
            {
                bool inserted = false;
                foreach (var b in batches)
                {
                    int membersInBatch = _dbContext.QurbaniQueue.Where(e => e.QurbaniBatchId == b.QurbaniBatchId).Count();
                    if (membersInBatch < b.Capacity && !b.Stopped)
                    {
                        _dbContext.QurbaniQueue.Add(new QurbaniQueue
                        {
                            FamilyHeadId = id,
                            QurbaniBatchId = b.QurbaniBatchId,
                            StartTime = DateTime.Now
                        });

                        _dbContext.SaveChanges();
                        batch = b;
                        inserted = true;
                        break;
                    }

                }
                if (!inserted)
                {
                    int batchNumber = _dbContext.QurbaniBatches.Count() + 1;

                    _dbContext.QurbaniBatches.OrderByDescending(e => e.QurbaniBatchId).FirstOrDefault().Stopped = true;
                    _dbContext.SaveChanges();

                    batch = new QurbaniBatch
                    {
                        Name = "Qurbani Batch #" + batchNumber,
                        Capacity = CAPACITY_LIMIT,
                        Completed = false

                    };
                    batch = AssignQrAndLink(batch);

                    _dbContext.QurbaniBatches.Add(batch);
                    _dbContext.SaveChanges();

                    _dbContext.QurbaniQueue.Add(new QurbaniQueue
                    {
                        FamilyHeadId = id,
                        QurbaniBatchId = batch.QurbaniBatchId,
                        StartTime = DateTime.Now
                    });

                    _dbContext.SaveChanges();
                }
            }

            return Ok(new
            {
                status = 200,
                batchName = batch?.Name,
                WhatsappGroupLink = batch?.WhatsappGroupLink
            });
        }

        public IActionResult CheckTimer(int id)
        {
            var fh = _dbContext.QurbaniQueue.Select(e => new
            {
                e.FamilyHeadId,
                e.StartTime,
                e.QurbaniBatch.DurationInMinutes
            }).FirstOrDefault(e => e.FamilyHeadId == id);

            var qurbaniTime = fh.StartTime.Add(TimeSpan.FromMinutes(fh.DurationInMinutes));
            ViewBag.QurbaniTime = qurbaniTime;

            return View();
        }

        public JsonResult GetQurbaniDoneCount()
        {
            return Json(_dbContext
                .QurbaniQueue
                .Select(e => new
                {
                    e.FamilyHead.Trashed,
                    e.QurbaniBatch.Completed,
                    e.FamilyHeadId,
                    e.QurbaniBatchId
                }).Where(e => !e.Trashed && e.Completed)
                .Count());
        }

        public JsonResult GetBookingCount()
        {
            List<BookingCountModel> model = new();

            var queue = from qq in _dbContext.QurbaniQueue
                        join qb in _dbContext.QurbaniBatches on qq.QurbaniBatchId equals qb.QurbaniBatchId
                        group qq by new
                        {
                            qb.Name,
                            qq.QurbaniBatchId,
                            qb.Capacity,
                            qb.Stopped,
                            qb.WhatsappGroupLink,
                            qb.WhatsappQrCodePath
                        } into g
                        orderby g.Key.QurbaniBatchId descending
                        select new
                        {
                            QurbaniBatchId = g.Key.QurbaniBatchId,
                            MemberCount = g.Count(),
                            g.Key.Capacity,
                            g.Key.Stopped,
                            g.Key.Name,
                            g.Key.WhatsappGroupLink,
                            g.Key.WhatsappQrCodePath,

                        };

            return Json(queue);
        }

        public IActionResult IndividualReceipt(int id)
        {
            QurbaniIndividualReceiptModel model = new();

            _dbContext.QurbaniQueue
                    .Include(e => e.FamilyHead)
                    .ThenInclude(e => e.Package)
                    .ThenInclude(e => e.Company)
                    .FirstOrDefault(e => e.FamilyHeadId == id);

            return View(model);
        }

        public IActionResult Enable(bool enable)
        {
            _dbContext.ApplicationSettings
                .Where(s => s.HajjSeasonsId == Convert.ToUInt32(_http.HttpContext.Session.GetString("ActiveSeasonId")))
                .FirstOrDefault().EnableQurbani = enable;
            _dbContext.SaveChanges();
            return Ok();
        }
    }
}

namespace TMG.Models.QurbaniModels
{
    public record QurbaniIndividualReceiptModel
    {
        public int HeadId { get; set; }
        public string Name { get; set; }
        public string FcId { get; set; }
        public string CID { get; set; }
        public string MobNumber { get; set; }
        public string KSANumber { get; set; }
        public string TokenNumber { get; set; }
    }
    public record BookingCountModel
    {
        public int QurbaniBatchId { get; set; }
        public int Count { get; set; }
    }
}
