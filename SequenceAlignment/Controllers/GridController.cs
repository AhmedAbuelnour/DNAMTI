using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using DataAccessLayer.Services;
using DataAccessLayer.Models;
using SequenceAlignment.Services;
using System.Net.Mail;
using System.Net;

namespace SequenceAlignment.Controllers
{
    [Route("api/[controller]")]
    public class GridController : Controller
    {
        private readonly IRepository Repo;
        public GridController(IRepository _Repo)
        {
            Repo = _Repo;
        }
        [HttpGet("[action]")]
        public string GetPendingAlignments()
        {
            return JsonConvert.SerializeObject(Repo.GetPendingAlignmentJobs().ToList());
        }
        [HttpGet("[action]/{JobId}")]
        public string GetPendingAlignment(string JobId)
        {
            return Serializer.EncryptByte(Repo.GetAlignmentJobById(JobId).ByteText, "GridComputing");
        }
        [HttpPost("[action]")]
        public void Result([FromBody]string JobResult)
        {
            AlignmentResult AlignmentResultFormGrid = JsonConvert.DeserializeObject<AlignmentResult>(JobResult);
            string Decrypted = Serializer.Decrypt(AlignmentResultFormGrid.Result, "ResultComputing");
            Repo.FinalizeJob(AlignmentResultFormGrid.JobId, Decrypted);
            MailMessage EMailMessage = new MailMessage("mtidna2017@gmail.com", AlignmentResultFormGrid.Email);
            EMailMessage.Subject = "Grid Notification";
            EMailMessage.IsBodyHtml = true;
            EMailMessage.Body = $"We Have done your Alignment Job, you can check it out in your history page! <a href='http://mtidna.azurewebsites.net/Display/{AlignmentResultFormGrid.JobId}'> Check my History Now </a>";
            SmtpClient SC = new SmtpClient("smtp.gmail.com", 587);
            SC.DeliveryMethod = SmtpDeliveryMethod.Network;
            SC.Credentials = new NetworkCredential("mtidna2017@gmail.com", "Mti_dna2017");
            SC.EnableSsl = true;
            SC.Send(EMailMessage);
        }
    }
}
