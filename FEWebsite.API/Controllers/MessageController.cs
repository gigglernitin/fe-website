using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using AutoMapper;

using FEWebsite.API.Interfaces;
using FEWebsite.API.DTOs.UserDTOs;
using FEWebsite.API.Helpers;
using FEWebsite.API.Models.ManyToManyModels.ComboModels;

namespace FEWebsite.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [Route("api/user/{userId}/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private IUserRepoService UserRepoService { get; }
        private IMapper Mapper { get; }
        private IUnitOfWork UnitOfWork { get; }

        public MessageController(IUserRepoService userRepoService, IMapper mapper, IUnitOfWork unitOfWork)
        {
            this.UserRepoService = userRepoService;
            this.Mapper = mapper;
            this.UnitOfWork = unitOfWork;
        }

        [HttpGet("{messageId}", Name = "GetUserMessage")]
        public async Task<IActionResult> GetUserMessage(int userId, int messageId)
        {
            var unauthorization = this.CheckIfLoggedInUserIsAuthorized(userId, "You aren't authorized to view this message.");
            if (unauthorization != null)
            {
                return unauthorization;
            }

            var userMessage = await this.UserRepoService.GetMessage(messageId).ConfigureAwait(false);

            if (userMessage == null)
            {
                return this.NotFound(new StatusCodeResultReturnObject(this.NotFound(),
                    "No message found."));
            }

            return this.Ok(userMessage);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserMessages(int userId, [FromQuery] MessageParams messageParams)
        {
            var unauthorization = this.CheckIfLoggedInUserIsAuthorized(userId, "You aren't authorized to view these messages.");
            if (unauthorization != null)
            {
                return unauthorization;
            }

            messageParams.UserId = userId;
            var pagedUserMessages = await this.UserRepoService.GetMessagesForUser(messageParams)
                .ConfigureAwait(false);

            var userMessages = this.Mapper.Map<IEnumerable<UserMessageToReturnDto>>(pagedUserMessages);

            Response.AddPagination(pagedUserMessages);

            return this.Ok(userMessages);
        }

        [HttpGet("thread/{recipientId}")]
        public async Task<IActionResult> GetUserMessageThread(int userId, int recipientId)
        {
            var unauthorization = this.CheckIfLoggedInUserIsAuthorized(userId, "You aren't authorized to view these messages.");
            if (unauthorization != null)
            {
                return unauthorization;
            }

            var messageThread = await this.UserRepoService.GetMessageThread(userId, recipientId).ConfigureAwait(false);

            var returnMessageThread = this.Mapper.Map<IEnumerable<UserMessageToReturnDto>>(messageThread);

            return this.Ok(returnMessageThread);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserMessage(int userId, UserMessageCreationDto userMessageCreationDto)
        {
            var unauthorization = this.CheckIfLoggedInUserIsAuthorized(userId, "You aren't authorized to send this message.");
            if (unauthorization != null)
            {
                return unauthorization;
            }

            userMessageCreationDto.SenderId = userId;

            var recipient = await this.UserRepoService.GetUser(userMessageCreationDto.RecipientId).ConfigureAwait(false);
            if (recipient == null)
            {
                return this.BadRequest(new StatusCodeResultReturnObject(this.BadRequest(),
                    "The recipent could not be found."));
            }

            var outgoingMessage = this.Mapper.Map<UserMessage>(userMessageCreationDto);
            this.UserRepoService.Add(outgoingMessage);

            if (await this.UnitOfWork.SaveAllAsync().ConfigureAwait(false))
            {
                var sender = await this.UserRepoService.GetUser(userMessageCreationDto.SenderId).ConfigureAwait(false);
                if (sender != null)
                {
                    outgoingMessage.Sender = sender;
                }

                var returnMessageObj = this.Mapper.Map<UserMessageToReturnDto>(outgoingMessage);
                return this.CreatedAtRoute("GetUserMessage", new { userId, messageId = outgoingMessage.Id },
                    returnMessageObj);
            }

            throw new DbUpdateException("The User Message failed to be created.");
        }

        [HttpPost("{messageId}")]
        public async Task<IActionResult> DeleteUserMessage(int messageId, int userId)
        {
            var unauthorization = this.CheckIfLoggedInUserIsAuthorized(userId, "You aren't authorized to delete this message.");
            if (unauthorization != null)
            {
                return unauthorization;
            }

            var messageToDelete = await this.UserRepoService.GetMessage(messageId).ConfigureAwait(false);

            if (messageToDelete.SenderId == userId)
            {
                messageToDelete.SenderDeleted = true;
            }
            if (messageToDelete.RecipientId == userId)
            {
                messageToDelete.RecipientDeleted = true;
            }

            if (messageToDelete.SenderDeleted && messageToDelete.RecipientDeleted)
            {
                this.UserRepoService.Delete(messageToDelete);
            }

            if (await this.UnitOfWork.SaveAllAsync().ConfigureAwait(false))
            {
                return this.NoContent();
            }

            throw new DbUpdateException("The User Message failed to be deleted.");
        }

        [HttpPost("{messageId}/read")]
        public async Task<IActionResult> MarkMessageAsRead(int userId, int messageId)
        {
            var unauthorization = this.CheckIfLoggedInUserIsAuthorized(userId, "You aren't authorized to delete this message.");
            if (unauthorization != null)
            {
                return unauthorization;
            }

            var message = await this.UserRepoService.GetMessage(messageId).ConfigureAwait(false);

            if (message.RecipientId != userId)
            {
                return this.Unauthorized("Senders cannot mark recipient messages as read.");
            }

            message.IsRead = true;
            message.DateRead = DateTime.Now;

            await this.UnitOfWork.SaveAllAsync().ConfigureAwait(false);

            return this.NoContent();
        }
    }
}
