﻿/*
 * Morgan Stanley makes this available to you under the Apache License,
 * Version 2.0 (the "License"). You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0.
 *
 * See the NOTICE file distributed with this work for additional information
 * regarding copyright ownership. Unless required by applicable law or agreed
 * to in writing, software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
 * or implied. See the License for the specific language governing permissions
 * and limitations under the License.
 */

using Crossroads.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;

namespace Crossroads.Services
{
    public class LaunchApplicationService : ILaunchApplicationService
    {
        private readonly PackageOption launcherOption;
        private readonly IFileSystem fileSystem;
        private readonly IProcessService processService;

        public LaunchApplicationService(IConfiguration configuration, IFileSystem fileSystem, IProcessService processService)
        {
            this.launcherOption = new PackageOption
            {
                Command = configuration["Launcher:Command"],
                Args = configuration["Launcher:Args"],
                Include = configuration.GetSection("Launcher:Include")?.Get<IEnumerable<string>>()
            };
            this.fileSystem = fileSystem;
            this.processService = processService;
        }

        public async Task<int> RunAsync(string arguments = null)
        {
            //string command = launcherOption.Command;
            string command = getCommand(launcherOption.Command);

            Console.WriteLine("commanddddddddddd");
            Console.WriteLine(command);

            if (string.IsNullOrWhiteSpace(command))
            {
                throw new Exception("Command is not configured correctly.");
            }

            string workingDirectory;
            if (fileSystem.Directory.Exists(assetsDirectory))
            {
                string tmpCommand = Path.Combine(hasSingleIncludeDirectory ? singleAssetsDirectory : assetsDirectory, command);
                if (fileSystem.File.Exists(tmpCommand))
                {
                    command = tmpCommand;
                }

                var res = Path.GetFileName(tmpCommand);
                Console.WriteLine("filename");
                Console.WriteLine(res);

                workingDirectory = assetsDirectory;
            }
            else
            {
                workingDirectory = null;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = string.IsNullOrWhiteSpace(arguments) ? launcherOption.Args : arguments,
                UseShellExecute = false,
                WorkingDirectory = workingDirectory
            };
            return await processService.RunAsync(startInfo);
        }

        private string assetsDirectory => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets");
        private string singleAssetsDirectory => Directory.GetDirectories(assetsDirectory).FirstOrDefault();
        private bool hasSingleIncludeDirectory => launcherOption.Include.Count() == 1;
        private string getCommand(string command)
        {
            if (command.Contains(Path.DirectorySeparatorChar) || command.Contains(Path.AltDirectorySeparatorChar))
            {
                return Path.GetFileName(command);
            }
            return command;
        }
    }
}
