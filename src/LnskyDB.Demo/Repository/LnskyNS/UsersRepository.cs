﻿using LnskyDB;
using LnskyDB.Demo.Entity.LnskyNS;
using LnskyDB.Demo.RepositoryInterface.LnskyNS;

namespace LnskyDB.Demo.Repository.LnskyNS
{
    public class UsersRepository : Repository<UsersEntity>, IUsersRepository
    {
    }
}

