using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Scani.Database.Entities;

[Index(nameof(Text), IsUnique = true)]
public class ScanCode
{
    public ScanCode(string text)
    {
        Text = text;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    protected ScanCode()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    { }

    [Key]
    public int ScanCodeId { get; set; }
    public byte[]? QrCodeBytes { get; set; }
    public byte[]? BarCodeBytes { get; set; }
    [Required]
    public string Text { get; set; }
}
