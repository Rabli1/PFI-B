using JSON_DAL;
using System;

namespace PhotosManager.Models
{
    public sealed class DB
    {
        #region singleton setup
        private static readonly DB instance = new DB();
        public static DB Instance
        {
            get
            {
                return instance;
            }
        }
        #endregion
        #region Repositories

        static public UsersRepository Users { get; set; }
            = new UsersRepository();

        static public PhotosRepository Photos { get; set; }
            = new PhotosRepository();

        static public LikesRepository Likes { get; set; }
           = new LikesRepository();

        static public LoginsRepository Logins { get; set; }
            = new LoginsRepository();

        static public CommentsRepository Comments { get; set; }
            = new CommentsRepository();

        static public Repository<UnverifiedEmail> UnverifiedEmails { get; set; }
            = new Repository<UnverifiedEmail>();

        static public Repository<RenewPasswordCommand> RenewPasswordCommands { get; set; }
            = new Repository<RenewPasswordCommand>();
        static public NotificationsRepository Notifications { get; set; }
            = new NotificationsRepository();
        #endregion
    }
}