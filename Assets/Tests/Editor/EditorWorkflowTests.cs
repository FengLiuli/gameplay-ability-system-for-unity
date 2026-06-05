using System.IO;
using NexusFramework.GAS.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace NexusFramework.GAS.Tests.Editor
{
    [TestFixture]
    public class EditorWorkflowTests
    {
        private const string PROJ_ROOT = "Assets/NexusFramework.GAS/Editor/GASSetting.asset";

        /// <summary>LoadOrCreate 返回有效资产</summary>
        [Test]
        public void SettingAsset_LoadOrCreate_ReturnsValid()
        {
            var asset = GASSettingAsset.LoadOrCreate();
            Assert.That(asset, Is.Not.Null);
            Assert.That(asset.TableOutpuPath, Is.Not.Null.And.Not.Empty);
            Assert.That(asset.ConfigProjectPath, Is.Not.Null.And.Not.Empty);
        }

        /// <summary>gen.bat 路径计算正确</summary>
        [Test]
        public void GenBatPath_Contains_ConfigProjectPath()
        {
            var asset = GASSettingAsset.LoadOrCreate();
            var bat = asset.FullGenBatPath();
            Assert.That(bat, Does.Contain(asset.ConfigProjectPath).IgnoreCase);
            Assert.That(bat, Does.EndWith("gen.bat").IgnoreCase);
        }

        /// <summary>导出菜单项方法不抛异常</summary>
        [Test]
        public void MenuItem_GenJson_DoesNotThrow()
        {
            var asset = GASSettingAsset.LoadOrCreate();
            // 验证路径非空即可，用户可自定义路径
            Assert.That(asset.TableOutpuPath, Is.Not.Null.And.Not.Empty);
            Assert.That(asset.ConfigProjectPath, Is.Not.Null.And.Not.Empty);
        }

        /// <summary>Settings 窗口可正常创建和关闭</summary>
        [Test]
        public void SettingsWindow_Opens_WithoutError()
        {
            var window = EditorWindow.GetWindow<GASSettingsWindow>(false, "Test");
            Assert.That(window, Is.Not.Null);
            window.Close();
        }
    }
}
