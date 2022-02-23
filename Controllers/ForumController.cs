using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenSourceHome.Models;

namespace OpenSourceHome.Controllers
{
    public class ForumController : Controller
    {
        private OpenSourceHomeContext db;
        public ForumController(OpenSourceHomeContext context)
        {
            db = context;
        }
        private int? uid
        {
            get
            {
                return HttpContext.Session.GetInt32("UserId");
            }
        }
        private bool isLoggedIn
        {
            get
            {
                return uid != null;
            }
        }
        [HttpGet("/forum/posts")]
        public IActionResult AllPosts()
        {
            if (!isLoggedIn)
            {
                return RedirectToAction("Index", "Home");
            }
            List<Post> allPosts = db.Posts
                .Include(post => post.Author)
                .Include(post => post.Likes)
                .ToList();
            return View("ForumAll", allPosts);
        }
        [HttpGet("/forum/post/new")]
        public IActionResult NewPost()
        {
            return View("NewPost");
        }
        [HttpPost("/forum/post/new/create")]
        public IActionResult Create(Post userPost)
        {
            if (!ModelState.IsValid)
            {
                return View("NewPost");
            }
            userPost.UserId = (int)uid;
            db.Posts.Add(userPost);
            db.SaveChanges();
            return RedirectToAction("AllPosts");
        }
        [HttpGet("/forum/post/{postId}")]
        public IActionResult Details(int postId)
        {
            if (!isLoggedIn)
            {
                return RedirectToAction("Index", "Home");
            }
            Post post = db.Posts
                .Include(p => p.Author)
                .Include(p => p.Likes)
                .ThenInclude(l => l.User)
                .Include(p => p.Replies)
                .FirstOrDefault(p => p.PostId == postId);
            if (post == null)
            {
                return RedirectToAction("AllPosts");
            }
            return View("Post", post);
        }
        [HttpPost("/forum/post/{postId}/like")]
        public IActionResult Like(int postId)
        {
            if (!isLoggedIn)
            {
                return RedirectToAction("Index", "Home");
            }
            UserPostLike existingLike = db.UserPostLikes
                .FirstOrDefault(l => l.UserId == (int)uid && l.PostId == postId);
            if (existingLike == null)
            {
                UserPostLike like = new UserPostLike()
                {
                    PostId = postId,
                    UserId = (int)uid
                };
                db.UserPostLikes.Add(like);
            }
            else
            {
                db.UserPostLikes.Remove(existingLike);
            }
            db.SaveChanges();
            return RedirectToAction("Details", postId);
        }
        [HttpPost("/forum/post/{postId}/reply")]
        public IActionResult Reply(Reply userInput)
        {
            if (!ModelState.IsValid)
            {
                return Redirect("/forum/post/" + userInput.PostId);
            }
            userInput.UserId = (int)uid;
            db.Replies.Add(userInput);
            db.SaveChanges();
            return Redirect("/forum/post/" + userInput.PostId);
        }
        [HttpPost("/forum/post/{postId}/delete")]
        public IActionResult Delete(int postId)
        {
            Post post = db.Posts.FirstOrDefault(p => p.PostId == postId);
            if (post == null || post.UserId != uid)
            {
                return RedirectToAction("AllPosts");
            }
            db.Posts.Remove(post);
            db.SaveChanges();
            return RedirectToAction("AllPosts");
        }
        [HttpPost("/forum/reply/{replyId}/delete")]
        public IActionResult DeleteReply(int replyId)
        {
            Reply reply = db.Replies.FirstOrDefault(r => r.ReplyId == replyId);
            if (reply == null || reply.UserId != uid)
            {
                return RedirectToAction("Details", new { postId = reply.PostId });
            }
            db.Replies.Remove(reply);
            db.SaveChanges();
            return RedirectToAction("Details", new { postId = reply.PostId });
        }
        [HttpGet("/forum/post/{postId}/edit")]
        public IActionResult Edit(int postId)
        {
            Post post = db.Posts.FirstOrDefault(p => p.PostId == postId);
            if (post == null || post.UserId != uid)
            {
                return RedirectToAction("AllPosts");
            }
            return View("EditPost", post);
        }
        [HttpPost("/forum/post/{postId}/update")]
        public IActionResult Update(int postId, Post editedPost)
        {
            if (!ModelState.IsValid)
            {
                editedPost.PostId = postId;
                return View("EditPost", editedPost);
            }
            Post dbPost = db.Posts.FirstOrDefault(p => p.PostId == postId);
            if (dbPost == null)
            {
                return RedirectToAction("AllPosts");
            }
            dbPost.Topic = editedPost.Topic;
            dbPost.Body = editedPost.Body;
            dbPost.UpdatedAt = DateTime.Now;
            db.Posts.Update(dbPost);
            db.SaveChanges();
            return RedirectToAction("Details", new { postId = postId });
        }
    }
}