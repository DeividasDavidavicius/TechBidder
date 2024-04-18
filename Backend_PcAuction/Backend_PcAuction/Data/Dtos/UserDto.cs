namespace Backend_PcAuction.Data.Dtos
{
    public record UserDto(string Id, string Username, string Address, string PhoneNumber, string BankDetails);
    public record UpdateUserDto(string Address, string PhoneNumber, string BankDetails);
}
