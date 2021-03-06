using Storage.Interfaces;
using Storage.Models.Follows;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Storage.Models
{
    [Table("Categories")]
    public class Category : AuditableModel, IDbMasterKey, IFollowable, IApproveable
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Book> Books { get; set; }
        public ICollection<CategoryFollow> Followers { get; set; }
        public bool Approved { get; set; }
    }
}
