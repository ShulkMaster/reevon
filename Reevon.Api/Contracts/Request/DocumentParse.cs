﻿namespace Reevon.Api.Contracts.Request;

public class DocumentParse
{
    public string Separator { get; set; } = "";
    public string Key { get; set; } = "";
    public IFormFile? Document { get; set; } = null;
}