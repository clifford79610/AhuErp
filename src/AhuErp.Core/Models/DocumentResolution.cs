using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AhuErp.Core.Models
{
    /// <summary>
    /// Резолюция руководителя на документ. От резолюции порождаются конкретные
    /// поручения (<see cref="DocumentTask"/>); сама резолюция — текстовое
    /// указание автора с датой издания.
    /// </summary>
    public class DocumentResolution
    {
        public int Id { get; set; }

        public int DocumentId { get; set; }
        public virtual Document Document { get; set; }

        public int AuthorId { get; set; }
        public virtual Employee Author { get; set; }

        [Required]
        [StringLength(2048)]
        public string Text { get; set; }

        public DateTime IssuedAt { get; set; }

        public virtual ICollection<DocumentTask> Tasks { get; set; }
            = new HashSet<DocumentTask>();
    }
}
