using UnityEngine;
using UnityEditor.PackageManager;
using System.Linq;
using UnityEditor.PackageManager.Requests;
using System.Threading.Tasks;

namespace Quickon.Core
{
    public class InstallRequiredPackages
    {
        internal async Task InstallPackages()
        {
            ListRequest listRequest = Client.List();
            await WaitUntilRequestCompletes(listRequest);

            if (listRequest.Status != StatusCode.Success)
            {
                Debug.LogError("Failed to list packages: " + listRequest.Error.message);
                return;
            }

            foreach (string packageName in RequiredInfo.Packages)
            {
                string packageNameWithoutVersion = packageName.Split('@')[0];
                UnityEditor.PackageManager.PackageInfo packageInfo = listRequest.Result.FirstOrDefault(p => p.name == packageNameWithoutVersion);

                if (packageInfo == null)
                {
                    AddRequest addRequest = Client.Add(packageName);
                    await WaitUntilRequestCompletes(addRequest);

                    if (addRequest.Status == StatusCode.Success)
                    {
                        Debug.Log($"Installed package: {packageName}");
                    }
                    else
                    {
                        Debug.LogError($"Failed to install package: {packageName}, Error: {addRequest.Error.message}");
                    }
                }
                else
                {
                    // Debug.Log($"Package already installed: {packageName}");
                }
            }
        }

        private async Task WaitUntilRequestCompletes(Request request)
        {
            while (!request.IsCompleted)
            {
                await Task.Yield();
            }
        }
    }
}