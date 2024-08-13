using System;
using System.Security.Claims;
using System.Security.Principal;
using BitPantry.Iota.Application.CRQS.Identity.Queries;
using MediatR;
using Microsoft.AspNetCore.Authentication;

// https://gunnarpeipman.com/aspnet-core-adding-claims-to-existing-identity/

namespace BitPantry.Iota.Web;

public class AddClaimsTransformation : IClaimsTransformation
{
    private IMediator _med;

    public AddClaimsTransformation(IMediator med) { _med = med; }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // see if the id is already added

        if(principal.Claims.Any(c => c.Type == ClaimTypes.Sid))
            return principal;
        
        // get the id from the email address claim value

        var emailClaim = principal.Claims.Single(c => c.Type == ClaimTypes.Email);
        var id = await _med.Send(new FindUserIdByEmailAddressQuery(emailClaim.Value));

        // if the id = 0, then the user is new - wait for the user to be created in the database
        // then the id can be set on the next request

        if(id == 0)
            return principal;
 
        // Add id claims to cloned identity

        var clone = principal.Clone();
        var newIdentity = (ClaimsIdentity)clone.Identity;

        newIdentity.AddClaim(new Claim(ClaimTypes.Sid, id.ToString()));
 
        return clone;
    }
}
