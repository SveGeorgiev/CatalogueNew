﻿using CatalogueNew.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using CatalogueNew.Models.Infrastructure;

namespace CatalogueNew.Models.Services.Like
{
    public class LikeDislikeService : BaseService, ILikeDislikeService
    {
        public LikeDislikeService(ICatalogueContext context)
            : base(context)
        {
        }

        public async Task Add(LikesDislike like)
        {
            var userLike = await this.Context.LikesDislikes.Where(l => l.UserID == like.UserID && l.ProductID == like.ProductID).FirstOrDefaultAsync();

            if (userLike == null)
            {
                this.Context.LikesDislikes.Add(like);
                await this.Context.SaveChangesAsync();
            }
            else
            {
                userLike.IsLike = like.IsLike;
                this.Context.Entry(userLike).State = EntityState.Modified;
                await this.Context.SaveChangesAsync();
            }
        }

        private async Task<List<LikesDislike>> All(int productID)
        {
            return await this.Context.LikesDislikes.Where(l => l.ProductID == productID).ToListAsync();
        }

        public async Task<LikeDislikeWrapper> GetLikesDislikesCount(int productID, string userID)
        {
            var likesDislikes = await All(productID);
            var like = likesDislikes.Where(ld => ld.UserID == userID).FirstOrDefault();
            var likes = likesDislikes.Where(ld => ld.IsLike == true).ToList();
            var dislikes = likesDislikes.Where(ld => ld.IsLike == false).ToList();
            LikeDislikeWrapper likeDislikeWrapper;

            if (like != null)
            {
                likeDislikeWrapper = new LikeDislikeWrapper(like.IsLike, likes.Count, dislikes.Count);
            }
            else
            {
                likeDislikeWrapper = new LikeDislikeWrapper(null, likes.Count, dislikes.Count);
            }

            return likeDislikeWrapper;
        }
    }
}
