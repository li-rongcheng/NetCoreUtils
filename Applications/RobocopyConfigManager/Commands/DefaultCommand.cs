﻿using NetCoreUtils.ProcessUtils;
using NetCoreUtils.Text.Json;
using RobocopyConfigManager.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp.Cli.Commands
{
    class DefaultCommand
    {
        public void RunBackup(string groupName)
        {
            const string robocopy = "robocopy";
            if (!ProcUtil.Exists(robocopy))
            {
                Console.Error.WriteLine($"Cannot find {robocopy}, program terminated");
                return;
            }

            var config = JsonConfigOperator<RobocopyConfig>.LoadCreate(RobocopyConfigParameters.fullConfigFilePath);

            foreach (var group in config.BackupGroups)
            {
                if(group.GroupName == groupName)
                {
                    string flags = $"/MT:16 /R:1 /W:3 /MIR /FFT /NP /LOG+:recover_{groupName}-{new Random().Next(0,99999)}.log";

                    bool firstItem = true;
                    foreach(var backup in group.Backups)
                    {
                        if(!firstItem)
                        {
                            Console.WriteLine("-----------------------------------------------");
                        }
                        string arguments = $"\"{backup.Source}\" \"{backup.Target}\" {flags}";
                        Console.WriteLine($"Executing: {robocopy} {arguments}");
                        ProcUtil.Run(robocopy, arguments);
                        firstItem = false;
                    }
                }
            }
        }

        //public void Test()
        //{
        //    Console.WriteLine("test");
        //}
    }
}