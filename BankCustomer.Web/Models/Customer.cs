using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;



public class Customer
{

    [DisplayName("Customer Name")]
    [NotMapped]
    public string FullName { get; set; }

    [JsonPropertyName("customer_number")]
    [DisplayName("Account Number")]
    public int customer_number { get; set; }

    [DisplayName("First Name")]
    [JsonPropertyName("first_name")]
    public string first_name { get; set; }

    [DisplayName("Last Name")]
    [JsonPropertyName("last_name")]
    public string last_name { get; set; }

    [DisplayName("DOB")]
    [JsonPropertyName("date_birth")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]

    public DateTime date_birth { get; set; }

    [DisplayName("Age")]
    public int age { get; set; }

    [DisplayName("SSN")]
    [JsonPropertyName("ssn")]
    public string ssn { get; set; }

    [DisplayName("SSN")]
    [JsonPropertyName("ssn")]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "****-**-****")]
    [NotMapped]
    public string HiddenSSN { get; set; }

    [DisplayName("Email")]
    [JsonPropertyName("email")]
    public string email { get; set; }

    [DisplayName("Primary Address")]
    [JsonPropertyName("primary_address")]
    public PrimaryAddress primary_address { get; set; }

    [DisplayName("Mobile")]
    [JsonPropertyName("mobile_phone_number")]
    public string mobile_phone_number { get; set; }

    [DisplayName("Join Date")]
    [JsonPropertyName("join_date")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
    public DateTime join_date { get; set; }
}

public class PrimaryAddress
{
    [DisplayName("Address Line 1")]
    [JsonPropertyName("address_line_1")]
    public string address_line_1 { get; set; }

    [DisplayName("City")]
    [JsonPropertyName("city")]
    public string city { get; set; }

    [DisplayName("State")]
    [JsonPropertyName("state")]
    public string state { get; set; }

    [DisplayName("Zip")]
    [JsonPropertyName("zip_code")]
    [RegularExpression(@"^\d{5}$", ErrorMessage = "Please enter a valid 5 digit ZIP code.")]
    public string zip_code { get; set; }
}