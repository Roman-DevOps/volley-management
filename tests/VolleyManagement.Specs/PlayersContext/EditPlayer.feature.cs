﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:2.3.0.0
//      SpecFlow Generator Version:2.3.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace VolleyManagement.Specs.PlayersContext
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.3.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public partial class EditPlayerFeature : Xunit.IClassFixture<EditPlayerFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
#line 1 "EditPlayer.feature"
#line hidden
        
        public EditPlayerFeature(EditPlayerFeature.FixtureData fixtureData, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Edit Player", "\tIn order to keep player information up to date\r\n\tAs a tournament administrator\r\n" +
                    "\tI want to be able to change player properties", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        public static void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        public virtual void TestInitialize()
        {
        }
        
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<Xunit.Abstractions.ITestOutputHelper>(_testOutputHelper);
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        void System.IDisposable.Dispose()
        {
            this.ScenarioTearDown();
        }
        
        [Xunit.FactAttribute(DisplayName="Edit existing player")]
        [Xunit.TraitAttribute("FeatureTitle", "Edit Player")]
        [Xunit.TraitAttribute("Description", "Edit existing player")]
        public virtual void EditExistingPlayer()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Edit existing player", ((string[])(null)));
#line 6
this.ScenarioSetup(scenarioInfo);
#line 7
    testRunner.Given("John Smith player exists", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 8
    testRunner.And("first name changed to Jack", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 9
    testRunner.When("I execute EditPlayer", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 10
    testRunner.Then("player is saved with new name", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Change to very long name")]
        [Xunit.TraitAttribute("FeatureTitle", "Edit Player")]
        [Xunit.TraitAttribute("Description", "Change to very long name")]
        public virtual void ChangeToVeryLongName()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Change to very long name", ((string[])(null)));
#line 12
this.ScenarioSetup(scenarioInfo);
#line 13
    testRunner.Given("John Smith player exists", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 14
    testRunner.And("first name set to Looong name which should be more than 60 symbols", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 15
    testRunner.When("I execute EditPlayer", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 16
    testRunner.Then("EntityInvariantViolationException is thrown", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Edit missing player")]
        [Xunit.TraitAttribute("FeatureTitle", "Edit Player")]
        [Xunit.TraitAttribute("Description", "Edit missing player")]
        public virtual void EditMissingPlayer()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Edit missing player", ((string[])(null)));
#line 18
this.ScenarioSetup(scenarioInfo);
#line 19
    testRunner.Given("Ivan Ivanov player does not exist", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 20
    testRunner.And("first name changed to Jack", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 21
    testRunner.When("I execute EditPlayer", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 22
    testRunner.Then("MissingEntityException is thrown", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.3.0.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : System.IDisposable
        {
            
            public FixtureData()
            {
                EditPlayerFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                EditPlayerFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion
