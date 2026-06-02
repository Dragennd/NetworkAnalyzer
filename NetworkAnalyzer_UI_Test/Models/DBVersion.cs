using SQLite;

namespace NetworkAnalyzer_UI_Test.Models
{
    [Table("DBVersion")]
    internal class DBVersion
    {
        [PrimaryKey]
        [Column("Version")]
        public string Version { get; set; }
    }
}
