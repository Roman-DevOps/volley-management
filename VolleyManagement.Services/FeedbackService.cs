﻿namespace VolleyManagement.Services
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using Crosscutting.Contracts.Providers;
    using Domain.FeedbackAggregate;
    using VolleyManagement.Crosscutting.Contracts.MailService;
    using VolleyManagement.Domain.UsersAggregate;

    /// <summary>
    /// Represents an implementation of IFeedbackService contract.
    /// </summary>
    public class FeedbackService : IFeedbackService
    {
        #region Fields

        private readonly IFeedbackRepository _feedbackRepository;

        private readonly IMailService _mailService;

        private readonly IUserService _userService;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackService"/> class.
        /// </summary>
        /// <param name="feedbackRepository"> The feedback repository</param>
        /// <param name="mailService">Mail service.</param>
        /// <param name="userService">User service.</param>
        public FeedbackService(
            IFeedbackRepository feedbackRepository,
            IMailService mailService,
            IUserService userService)
        {
            _feedbackRepository = feedbackRepository;
            _mailService = mailService;
            _userService = userService;
        }

        #endregion

        #region Implementation

        /// <summary>
        /// Creates feedback.
        /// </summary>
        /// <param name="feedbackToCreate">Feedback to create.</param>
        public void Create(Feedback feedbackToCreate)
        {
            if (feedbackToCreate == null)
            {
                throw new ArgumentNullException("feedback");
            }

            UpdateFeedbackDate(feedbackToCreate);
            _feedbackRepository.Add(feedbackToCreate);
            _feedbackRepository.UnitOfWork.Commit();

            NotifyUser(feedbackToCreate.UsersEmail);
            NotifyAdmins(feedbackToCreate);
        }

        #endregion

        #region Privates

        private void UpdateFeedbackDate(Feedback feedbackToUpdate)
        {
            feedbackToUpdate.Date = TimeProvider.Current.UtcNow;
        }

        /// <summary>
        /// Send a confirmation email to user.
        /// </summary>
        /// <param name="emailTo">Recipient email.</param>
        private void NotifyUser(string emailTo)
        {
            string body = Properties.Resources.FeedbackConfirmationLetterBody;
            string subject = Properties.Resources.FeedbackConfirmationLetterSubject;

            EmailMessage emailMessage = new EmailMessage(emailTo, subject, body);
            _mailService.Send(emailMessage);
        }

        /// <summary>
        /// Send a feedback email to all admins.
        /// </summary>
        /// <param name="feedback">Feedback to send.</param>
        private void NotifyAdmins(Feedback feedback)
        {
            string subject = string.Format(
                Properties.Resources.FeedbackEmailSubjectToAdmins,
                feedback.Id);

            string body = string.Format(
                Properties.Resources.FeedbackEmailBodyToAdmins,
                feedback.Id,
                feedback.Date,
                feedback.UsersEmail,
                feedback.Status,
                feedback.Content);

            IList<User> adminsList = _userService.GetAdminsList();

            foreach (var admin in adminsList)
            {
                EmailMessage emailMessage = new EmailMessage(admin.Email, subject, body);
                _mailService.Send(emailMessage);
            }
        }
        #endregion
    }
}