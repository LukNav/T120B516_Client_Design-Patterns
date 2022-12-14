using System.Text.Json;
using Microsoft.Net.Http.Headers;
using WindowsFormsApplication.Controllers;
using WindowsFormsApplication.Controllers.InterpreterPattern;
using WindowsFormsApplication.Controllers.MediatorPattern;
using WindowsFormsApplication.Helpers;
using WindowsFormsApplication.Models;
using static System.Net.WebRequestMethods;

namespace WindowsFormsApplication
{
    public partial class MenuForm : Form
    {
        public static string PlayerName { get; private set; }
        private static MenuMediator Mediator { get; set; }

        public MenuForm()
        {
            InitializeComponent();
        }

        public void SubmitNameButton_Click(object sender, EventArgs e)
        {
            PlayerName = Program.MenuForm.NameTextBox.Text;
            GameForm.PlayerName = PlayerName;
            
            if (!string.IsNullOrEmpty(PlayerName))//if anything is entered in the name textbox
            {
                if (TryCreateClient(PlayerName, Program.LocalHostPort) == true)//try sending request to server and create a new player
                {
                    ToggleLoginItems(false);//Hide Ui login labels
                    ToggleReadyToPlayUIItems(true);//Show Ready to play button and label in UI
                }
                else
                    return;
            }
            else
                Program.MenuForm.ErrorLabel.Text = "Please enter a Name and then click Start Session";
        }

        public void ReadyButton_Click(object sender, EventArgs e)
        {
            SetPlayerAsReady(PlayerName);//Set Player as ready in Server
            ToggleReadyToPlayUIItems(false);//Hide Ready button
            ShowWaitingForPlayerLabel(true);//Show "Waiting for players" label

            //Now we wait for server's response that everybody is ready
        }

        public void StartGame(Game game)
        {
            Program.MenuForm.Visible = false;
            Program.GameForm.Visible = true;
            Program.GameForm.StartGame(game);
        }
        #region UI controls

        private void ToggleLoginItems(bool isVisible)
        {
            Program.MenuForm.SubmitNameButton.Visible = isVisible;
            Program.MenuForm.EnterNameLabel.Visible = isVisible;
            Program.MenuForm.NameTextBox.Visible = isVisible;
            Program.MenuForm.ErrorLabel.Visible = isVisible;
        }

        private void ToggleReadyToPlayUIItems(bool isVisible)
        {
            Program.MenuForm.ReadyToPlayButton.Visible = isVisible;
            Program.MenuForm.ReadyToPlayLabel.Visible = isVisible;
            Program.MenuForm.quitButton.Visible = isVisible;
        }

        private void ShowWaitingForPlayerLabel(bool isVisible)
        {
            Program.MenuForm.WaitingForPlayersLabel.Visible = isVisible;
        }

        #endregion

        #region HttpRequests

        private bool TryCreateClient(string name, string localhostPort)
        {
            CreateMediatorIfNeeded(name);

            string errorMessage = Mediator.CreateClient();
            if (errorMessage != null)
                Program.MenuForm.ErrorLabel.Text = errorMessage;
            return true;//Returns true if client was created
        }

        private static void CreateMediatorIfNeeded(string name)
        {
            if (Mediator == null)
                if (Program.ShouldUseProxy)
                    Mediator = new MenuMediator(name, Program.LocalHostPort, Program.ProxyIp);
                else
                    Mediator = new MenuMediator(name, Program.LocalHostPort);
        }

        private Game GetGameInfo()
        {
            return Mediator.GetGameInfo();
        }

        private void SetPlayerAsReady(string name)
        {
            CreateMediatorIfNeeded(name);
            Mediator.SetPlayerAsReady();
        }

        #endregion

        private void MenuForm_Load(object sender, EventArgs e)
        {
            this.Name = $"Menu: {Program.LocalHostPort}";
            this.Text = $"Menu: {Program.LocalHostPort}";
        }

        private void DebugButton_Click(object sender, EventArgs e)
        {
            string serverUrl = $"{Program.ServerIp}/Debug/StartGameSolo/{Program.LocalHostPort}";
            HttpRequests.GetRequest(serverUrl);
        }

        public void quitButton_Click(object sender, EventArgs e)
        {
            Mediator.UnregisterPlayer();
            ToggleLoginItems(true);//Hide Ui login labels
            ToggleReadyToPlayUIItems(false);//Show Ready to play button and label in UI
        }


        private void InterpreterTextBox_TextChanged(object sender, EventArgs e)
        {

        }
        MenuInterpreter menuInterpreter = new MenuInterpreter();
        private void InterpreterSubmit_Click(object sender, EventArgs e)
        {
            menuInterpreter.Interpret(new MenuContext(Program.MenuForm.InterpreterTextBox.Text));
        }
    }
}
