using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class MessagesController : BaseApiController
    {
        private readonly IMapper _mapper;
         private readonly IUnitOfWork _uoW ;
        public MessagesController(IUnitOfWork uoW , IMapper mapper)
        {
            _mapper = mapper;
            _uoW = uoW;

        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
                var username = User.Getusername();
                if(username == createMessageDto.RecipientUsername.ToLower())
                {
                    return BadRequest("You cannot send messages to yourself");
                }
                var sender = await _uoW.UserRepository.GetUserbyUsernameAsync(username);
                var recipient = await _uoW.UserRepository.GetUserbyUsernameAsync(createMessageDto.RecipientUsername);

                if(recipient == null) return NotFound();

                var message = new Message
                {
                    Sender = sender,
                    Recipient = recipient,
                    SenderUserName = sender.UserName,
                    RecipientUsername = recipient.UserName,
                    Content = createMessageDto.Content

                };

                _uoW.MessageRepository.AddMessage(message);
                if( await _uoW.Complete()) return Ok(_mapper.Map<MessageDto>(message));
                return BadRequest("Failed to send message");

        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.Getusername();
            var messages = await _uoW.MessageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader( new PaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages));
            return messages;
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
             var username = User.Getusername();
             var message = await _uoW.MessageRepository.GetMessage(id);
             if(message.SenderUserName != username && message.RecipientUsername != username) return Unauthorized();

             if(message.SenderUserName == username) message.SenderDeleted = true;
             if(message.RecipientUsername == username) message.RecipientDeleted = true;
            
            if(message.SenderDeleted && message.RecipientDeleted)
            {
                _uoW.MessageRepository.DeleteMessage(message);
            }

            if( await _uoW.Complete()) return Ok();
                return BadRequest("Problem deleting the message");
        }
    }
}