using SQLite;

namespace NetworkAnalyzer.Apps.Models
{
    [Table("DBVersion")]
    internal class DBVersion
    {
        [PrimaryKey]
        [Column("Version")]
        public string Version { get; set; }
    }
}
