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
        private readonly IUserRepository _userRepository ;
        private readonly ImessageRepository _messageRepository ;
        private readonly IMapper _mapper;
        public MessagesController(IUserRepository userRepository , ImessageRepository messageRepository , IMapper mapper)
        {
            _mapper = mapper;
            _messageRepository = messageRepository;
            _userRepository = userRepository;
            
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
                var username = User.Getusername();
                if(username == createMessageDto.RecipientUsername.ToLower())
                {
                    return BadRequest("You cannot send messages to yourself");
                }
                var sender = await _userRepository.GetUserbyUsernameAsync(username);
                var recipient = await _userRepository.GetUserbyUsernameAsync(createMessageDto.RecipientUsername);

                if(recipient == null) return NotFound();

                var message = new Message
                {
                    Sender = sender,
                    Recipient = recipient,
                    SenderUserName = sender.UserName,
                    RecipientUsername = recipient.UserName,
                    Content = createMessageDto.Content

                };

                _messageRepository.AddMessage(message);
                if( await _messageRepository.SaveAllAsync()) return Ok(_mapper.Map<MessageDto>(message));
                return BadRequest("Failed to send message");

        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.Getusername();
            var messages = await _messageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader( new PaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages));
            return messages;
        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
        {
            var currentUserName = User.Getusername();
            return Ok(await _messageRepository.GetMessageThread(currentUserName, username));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
             var username = User.Getusername();
             var message = await _messageRepository.GetMessage(id);
             if(message.SenderUserName != username && message.RecipientUsername != username) return Unauthorized();

             if(message.SenderUserName == username) message.SenderDeleted = true;
             if(message.RecipientUsername == username) message.RecipientDeleted = true;
            
            if(message.SenderDeleted && message.RecipientDeleted)
            {
                _messageRepository.DeleteMessage(message);
            }

            if( await _messageRepository.SaveAllAsync()) return Ok();
                return BadRequest("Problem deleting the message");
        }
    }
}