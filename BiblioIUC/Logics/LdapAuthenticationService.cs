using BiblioIUC.Logics.Interfaces;
using BiblioIUC.Models;
using Microsoft.Extensions.Options;
using Novell.Directory.Ldap;
using Serenity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiblioIUC.Logics
{
    public class LdapAuthenticationService : ILDAPAuthenticationService
    {
       

        private readonly LdapConfig _config;
        private readonly LdapConnection _connection;
        private readonly Dictionary<string, RoleOptions> _mapGroup;

        public LdapAuthenticationService(IOptions<LdapConfig> config)
        {
            _config = config.Value;
            _connection = new LdapConnection
            {
                SecureSocketLayer = true
            };
            _mapGroup = new Dictionary<string, RoleOptions>
            {
                {_config.MapGroupAdmin.ToLower(), RoleOptions.Admin },
                {_config.MapGroupLibrarian.ToLower(), RoleOptions.Librarian },
                {_config.MapGroupTeacher.ToLower(), RoleOptions.Teacher },
                {_config.MapGroupStudent.ToLower(), RoleOptions.Student }
            };
        }

        public UserLDAPModel Login(string username, string password)
        {
            try
            {
                var bindDn = string.Format(_config.BindDn, username);
                _connection.Connect(_config.Url, LdapConnection.DefaultSslPort);
                _connection.Bind(bindDn, password);
                if (_connection.Bound)
                {
                    var result = _connection.Search
                    (
                        bindDn,
                        LdapConnection.ScopeSub,
                        null,
                        new[] { _config.MapMemberOfAttribute, _config.MapDisplayNameAttribute, 
                            _config.MapFullNameAttribute, _config.MapUIDAttribute },
                        false
                    );

                    var user = result.Next();
                    if (user != null)
                    {

                        RoleOptions role = RoleOptions.Student;
                        foreach(var group in _mapGroup.Keys)
                        {
                            if (user.GetAttribute(_config.MapMemberOfAttribute).StringValue.ToLower().Contains(group))
                            {
                                role = _mapGroup[group];
                                break;
                            }
                        }
                        
                        return new UserLDAPModel
                        (
                            user.GetAttribute(_config.MapFullNameAttribute).StringValue,
                            user.GetAttribute(_config.MapDisplayNameAttribute).StringValue,
                            user.GetAttribute(_config.MapUIDAttribute).StringValue,
                            role == RoleOptions.Admin ? RoleOptions.Librarian : role
                        );

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _connection?.Disconnect();
            }
            return null;
        }
    }
    public class LdapConfig
    {
        public string Url { get; set; }
        public string BindDn { get; set; }
        public string MapGroupAdmin { get; set; }
        public string MapGroupLibrarian { get; set; }
        public string MapGroupTeacher { get; set; }
        public string MapGroupStudent { get; set; }
        public string MapMemberOfAttribute { get; set; }
        public string MapDisplayNameAttribute { get; set; }
        public string MapFullNameAttribute { get; set; }
        public string MapUIDAttribute { get; set; }
    }
}
