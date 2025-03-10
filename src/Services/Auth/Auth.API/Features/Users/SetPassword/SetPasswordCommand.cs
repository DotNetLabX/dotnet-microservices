namespace Auth.API.Features.Users.SetPassword;

public record SetPasswordCommand(string Email, string NewPassword, string ConfirmPassword, string TwoFactorToken);
public record SetPasswordResponse(string Email);