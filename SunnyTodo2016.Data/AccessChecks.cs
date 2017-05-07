using System;

namespace SunnyTodo2016.Data
{
    public class AccessChecks
    {
        public Burndown DbBurndown { get; set; }
        public Guid CurrentUserId { get; set; }



        public AccessChecks(Burndown dbBurndown, Guid currentUserId)
        {
            DbBurndown = dbBurndown;
            CurrentUserId = currentUserId;
        }

        private bool IsOwnedByCurrentUser => CurrentUserId == DbBurndown.OwnerUserId;

        public bool CanView
        {
            get
            {
                if (IsOwnedByCurrentUser) return true;
                if (DbBurndown.IsPublicViewable) return true;
                return false;
            }
        }

        public bool CanEdit
        {
            get
            {
                if (IsOwnedByCurrentUser) return true;
                if (DbBurndown.IsPublicEditable) return true;
                return false;
            }
        }

        public bool CanEditAccessibility
        {
            get
            {
                if (CurrentUserId == MyUser.AnonymousUserId) return false;
                if (IsOwnedByCurrentUser) return true;
                return false; 
            }
        }

        public bool IfAnonymouslyOwnedMustBePubliclyEditable
        {
            get
            {
                if (DbBurndown.OwnerUserId == MyUser.AnonymousUserId) return true;
                return false; 
            }
        }
    }
}