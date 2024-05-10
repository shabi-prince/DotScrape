using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Event : GenericModel
    {
        public string Name { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public string CoverPicture { get; set; }
        public string Address { get; set; }
        public string ShortDescription { get; set; }        
        public string Status { get; set; }
        public string EventDateTime { get; set; }
        [ForeignKey("Community")]
        public int CommunityId { get; set; }
        public virtual Community Community { get; set; }
    }
}
