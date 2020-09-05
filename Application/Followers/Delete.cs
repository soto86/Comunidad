﻿using Application.Errors;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Followers
{
    public class Delete
    {
        public class Command : IRequest
        {
            public string Username { get; set; }
        }
        public class Handler : IRequestHandler<Add.Command>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, IUserAccessor userAccessor)
            {
                _context = context;
                _userAccessor = userAccessor;
            }
            public async Task<Unit> Handle(Add.Command request,
                CancellationToken cancellationToken)
            {
                var observer = await _context.Users.SingleOrDefaultAsync(x
                    => x.UserName == _userAccessor.GetCurrentUserName(), cancellationToken: cancellationToken);

                var target = await _context.Users.SingleOrDefaultAsync(x
                    => x.UserName == request.Username, cancellationToken: cancellationToken);

                if (target == null)
                    throw new RestException(HttpStatusCode.NotFound, new { User = "Not found" });

                var following = await _context.Followings.SingleOrDefaultAsync(x
                    => x.ObserverId == observer.Id && x.TargetId == target.Id, cancellationToken: cancellationToken);

                if (following == null)
                    throw new RestException(HttpStatusCode.BadRequest,
                        new { User = "You are not following this user." });

                _context.Followings.Remove(following);

                var success = await _context.SaveChangesAsync( cancellationToken: cancellationToken) > 0;

                if (success) return Unit.Value;

                throw new Exception("Problem saving changes");
            }
        }
    }
}
