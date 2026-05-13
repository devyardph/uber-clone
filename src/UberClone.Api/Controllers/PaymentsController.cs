using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UberClone.Api.Services;
using UberClone.Shared.Dtos;

namespace UberClone.Api.Controllers;

[ApiController]
[Route("payments/methods")]
[Authorize]
public sealed class PaymentsController : ControllerBase
{
    private readonly IPaymentStore _store;
    public PaymentsController(IPaymentStore store) => _store = store;

    private string UserId => User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)!;

    [HttpGet]
    public ActionResult<IEnumerable<PaymentMethodDto>> List() => Ok(_store.ForUser(UserId));

    [HttpPost]
    public ActionResult<PaymentMethodDto> Add([FromBody] AddPaymentMethodRequest req) =>
        Ok(_store.Add(UserId, req));

    [HttpDelete("{id}")]
    public ActionResult Remove(string id) =>
        _store.Remove(UserId, id) ? NoContent() : NotFound();
}
