using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace net08.Models;

public class Message {
    public int Id { get; set; }
    
    public int SenderId { get; set; }
    public virtual User Sender { get; set; }
    
    public int ReceiverId { get; set; }
    public virtual User Receiver { get; set; }
    
    public string Text { get; set; }
    [DataType(DataType.Date)]
    [Column(TypeName = "Date")]
    public DateTime Date { get; set; }
}