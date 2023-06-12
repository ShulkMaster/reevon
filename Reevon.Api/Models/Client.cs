namespace Reevon.Api.Models;

public class Client
{
    public const string DocumentColumn = "documento";
    public const string NameColumn = "nombres";
    public const string LastNameColumn = "apellidos";
    public const string CardColumn = "tarjeta";
    public const string RankColumn = "tipo";
    public const string PhoneColumn = "telefono";
    public const string PoligoneColumn = "poligono";

    public static readonly string[] Columns =
    {
        DocumentColumn,
        NameColumn,
        LastNameColumn,
        CardColumn,
        RankColumn,
        PhoneColumn,
        PoligoneColumn
    };

    public string Document { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Card { get; set; } = string.Empty;
    public string Rank { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Poligone { get; set; } = string.Empty;
}