using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class LikesRepository : ILikesRepository
    {
        public DataContext _Context { get; }
        
        public LikesRepository(DataContext context)
        {
            _Context = context;
            
        }
        public async Task<UserLike> GetUserLike(int sourceUserId, int targetUserId)
        {
            return await _Context.Likes.FindAsync(sourceUserId, targetUserId);
        }

        public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams)
        {
            var users = _Context.Users.OrderBy(u => u.UserName).AsQueryable();
            var likes = _Context.Likes.AsQueryable();
            
            if(likesParams.predicate == "liked")
            {
                likes = likes.Where(like => like.SourceUserId ==likesParams.UserId);
                users = likes.Select(like => like.TargetUser);
            }

            if(likesParams.predicate == "likedBy")
            {
                likes = likes.Where(like => like.TargeUserId ==likesParams.UserId);
                users = likes.Select(like => like.SourceUser);
            }

            var likedUsers = users.Select(user => new LikeDto
            {
                UserName = user.UserName,
                KnownAs = user.KnownAs,
                Age = user.DateOfBirth.CalculateAge(),
                PhotoUrl = user.Photos.FirstOrDefault(x=> x.IsMain).Url,
                Id= user.Id

            });
            
            return await PagedList<LikeDto>.CreateAsync(likedUsers, likesParams.PageNumber, likesParams.PageSize);
        }
        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await _Context.Users
                .Include(x => x.LikedUsers)
                .FirstOrDefaultAsync( x => x.Id == userId);
        }
    }
}