using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CMSCore.Models
{
    public enum UserRole { Admin, Member,Guest }
    public enum ChatType { Room, Private }
    public enum ContributionStatus { Approved, Pending, Rejected } //for a value permanent.
    public enum FileType { doc, docx, img,
        Document
    }
    public enum Gender { Male , Female,Other}

    public static class _Global
    {
        private static string rootFolderName => "_Files";

        public static string PATH_TOPIC => Path.Combine(rootFolderName, "Topics");
        public static string PATH_TEMP { get { return Path.Combine(rootFolderName, "Temp"); } }
    }
    public class CUser : IdentityUser
    {
        public CUser() : base()
        {
            Chats = new List<ChatCUser>();
        }
        public ICollection<ChatCUser> Chats { get; set; }
        //
        public string UserNumber { get;set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public DateTime DOB { get; set; }
        public int? DepartmentId { get; set; }
        public virtual Department Department { get; set; }


        public virtual ICollection<Contribution> Contributions { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
    }

    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<CUser> User { get; set; }
    }

    public class Topic
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime DeadLine { get; set; }
        public DateTime DeadLine_2 { get; set; }
        public DateTime CreationDate { get; set; }


        public virtual ICollection<Contribution> Contributions { get; set; }
    }

    public class Contribution
    {
        public int Id { get; set; }
        public ContributionStatus Status { get; set; }

        public string ContributorId { get; set; }
        public virtual CUser Contributor { get; set; }


        public int TopicId { get; set; }
        public virtual Topic Topic { get; set; }

        public virtual ICollection<SubmittedFile> SubmittedFiles { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }

    }

    public class SubmittedFile
    {
        public int Id { get; set; }
        public string URL { get; set; }
        public FileType Type { get; set; }

        public int ContributionId { get; set; }
        public virtual Contribution Contribution { get; set; }
    }

    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; }

        public DateTime Date { get; set; }
        public string UserId { get; set; }
        public virtual CUser User { get; set; }


        public int ContributionId { get; set; }
        public virtual Contribution Contribution { get; set; }
    }
    public class API_Department_Contribution
    {
        public string DepartmentName { get; set; }
        public int TotalContribution { get; set; }
    }
    public class API_Status_Contribution
    {
        public string TopicName { get; set; }
        public int TotalApproved { get; set; }
        public int TotalPending { get; set; }
        public int TotalRejected { get; set; }
    }
    /// <summary>
    ///  Tạo chat 
    /// </summary>
    public class Chat
    {
        public Chat()
        {
            Messages = new List<Message>();
            Users = new List<ChatCUser>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public ChatType Type { get; set; }
        public ICollection<Message> Messages { get; set; }
        public ICollection<ChatCUser> Users { get; set; }
    }
    public class ChatCUser
    {
        public string UserId { get; set; }
        public CUser User { get; set; }
        public int ChatId { get; set; }
        public Chat Chat { get; set; }
        public UserRole Role { get; set; }
    }
    public class Message
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }

        public int ChatId { get; set; }
        public Chat Chat { get; set; }
    }
   
}
