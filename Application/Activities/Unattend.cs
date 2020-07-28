﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Application.Errors;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Activities
{
    public class Unattend
    {
        public class Command : IRequest
        {
            public Guid Id { get; set; }
        }
        public class Handler : IRequestHandler<Command>
        {
            private readonly DataContext _context;
            private readonly IUserAccesor _userAccesor;

            public Handler(DataContext context, IUserAccesor userAccesor)
            {
                _context = context;
                _userAccesor = userAccesor;
            }
            public async Task<Unit> Handle(Command request,
                CancellationToken cancellationToken)
            {
                var activity = await _context.Activities.FindAsync(request.Id);
                if (activity == null)
                {
                    throw new RestException(HttpStatusCode.NotFound,
                        new { Activity = "Could not found activity" });
                }

                var user = await _context.Users.SingleOrDefaultAsync(x =>
                    x.UserName == _userAccesor.GetCurrentUserName(), cancellationToken);

                var attendance =
                    await _context.UserActivities.SingleOrDefaultAsync(x =>
                        x.ActivityId == activity.Id && x.AppUserId == user.Id, cancellationToken);

                if (attendance == null)
                {
                    return Unit.Value;
                }

                if (attendance.IsHost)
                {
                    throw new RestException(HttpStatusCode.BadRequest, new {Attendance = "You cannot remove yourself as a host."});
                }

                _context.UserActivities.Remove(attendance);

                var success = await _context.SaveChangesAsync(cancellationToken) > 0;
                
                if (success) return Unit.Value;

                throw new Exception("Problem saving changes");
            }
        }
    }
}
