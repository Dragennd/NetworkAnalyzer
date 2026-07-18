using SQLite;

namespace NetworkAnalyzer.Models
{
    [Table("DBVersion")]
    internal class DBVersion
    {
        [PrimaryKey]
        [Column("Version")]
        public string Version { get; set; }
    }
}
