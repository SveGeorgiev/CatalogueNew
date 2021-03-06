﻿using CatalogueNew.Models.Entities;
using CatalogueNew.Models.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogueNew.Models.Services
{
    public class CommentService : BaseService, ICommentService
    {
        public CommentService(ICatalogueContext context)
            : base(context)
        {
        }

        public async Task Add(Comment comment)
        {
            this.Context.Comments.Add(comment);
            await this.Context.SaveChangesAsync();
        }

        public async Task Modify(Comment comment)
        {
            var dataComment = await this.Context.Comments.Where(c => c.CommentID == comment.CommentID).FirstOrDefaultAsync();
            dataComment.Text = comment.Text;
            dataComment.TimeStamp = DateTime.UtcNow;

            this.Context.Entry(dataComment).State = EntityState.Modified;
            await this.Context.SaveChangesAsync();
        }

        public async Task Remove(int id)
        {
            await this.Context.Database.ExecuteSqlCommandAsync("usp_deleteComments @id = {0}", id);
        }

        public async Task<IEnumerable<CommentWrapper>> CommentsByProduct(int productID)
        {
            var commentsByProduct = await this.Context.Comments
                .Where(c => c.ProductID == productID)
                .Include(c => c.User)
                .ToListAsync();

            var commentsWrapperList = new List<CommentWrapper>();

            foreach (var comment in commentsByProduct)
            {
                if (comment.ParentCommentID == null)
                {
                    commentsWrapperList.Add(new CommentWrapper()
                    {
                        ParentComment = comment,
                        ChildrenComments = new List<CommentWrapper>(0)
                    });
                }
            }

            SetChildren(commentsWrapperList, commentsByProduct);

            return commentsWrapperList;
        }

        private void SetChildren(List<CommentWrapper> commentsWrapper, List<Comment> commentsByProduct)
        {
            foreach (var commentWrapper in commentsWrapper)
            {
                var children = commentsByProduct.Where(x => x.ParentCommentID == commentWrapper.ParentComment.CommentID).ToList();

                foreach (var child in children)
                {
                    commentWrapper.ChildrenComments.Add(new CommentWrapper()
                    {
                        ParentComment = child,
                        ChildrenComments = new List<CommentWrapper>(0)
                    });
                }
                SetChildren(commentWrapper.ChildrenComments, commentsByProduct);
            }
        }
    }
}
