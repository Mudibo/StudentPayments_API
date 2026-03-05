using System.Runtime.Serialization;
namespace StudentPayments_API.Models.Enums;

public enum DuesTypeEnum
{
    [EnumMember(Value = "Tuition")]
    Tuition,
    [EnumMember(Value = "Library")]
    Library,
    [EnumMember(Value = "Hostel")]
    Hostel,
    [EnumMember(Value = "Sports")]
    Sports,
    [EnumMember(Value = "Other")]
    Other,
    [EnumMember(Value = "Lab")]
    Lab
}