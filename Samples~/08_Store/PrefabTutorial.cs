using UnityEngine.GameFoundation.DefaultLayers;
using UnityEngine.UI;

namespace UnityEngine.GameFoundation.Sample
{
    public class PrefabTutorial : MonoBehaviour
    {
        private bool m_WrongDatabase;
        private int m_CurrentStep;
        private Button m_NextButton;
        private Button m_PreviousButton;
        private Text m_CurrentStepTitle;
        private Text m_CurrentStepDisplay;
        private readonly string[] m_TutorialTitles =
        {
            "Setting up your Store",
            "Adding Currency Item prefabs",
            "Changing the color of Currency Item prefabs",
            "Adding Store prefab",
            "Customizing Transaction Item prefabs",
            "Adding customized Transaction Item prefab",
            "Triggering the Store prefab display to update",
            "Creating your own Purchase Button part 1",
            "Creating your own Purchase Button part 2",
            "Changing the Purchase Button on a Transaction Item",
            "The End"
        };
        private readonly string[] m_TutorialSteps = 
        {
            "This is an example of how to build a Store using Game Foundation's <b>Store System</b> and <b>Store</b> <i>prefabs</i>. Transactions and Stores will need to be set up in the Game Foundation Store Window, however, for this example, we have set some up for you.",
            "In \"<i>Game Foundation/UI/Prefabs/Indicators</i>\" you will find a <b>Currency Item</b> <i>prefab</i>. Drag two copies of it into the <b>Header</b> <i>GameObject</i> under <b>Store UI</b> <i>GameObject</i> and arrange to your preference in the Scene view. In the Inspector window under <b>Currency View</b> component select 'Coin' and 'Gem' for the Currency, respectively.",
            "You can change the size or background of the <b>Currency Item</b>. Let's change the background by changing the color in the Image component. This could be changed per instance, or, when not at runtime, in the prefab to apply to all Currency Items.",
            "Now add a <b>Store</b> <i>prefab</i> of your choice to the <b>Body</b> <i>GameObject</i> under <b>Store UI</b> <i>GameObject</i>. In the Inspector window under <b>Store View</b> <i>component</i> select the store and tag. You can see how when you switch them the Store display changes to match. (Note: The Store prefabs will only generate the Transaction Items at runtime.)",
            "You can also change the prefab that the Store uses when generating its list of transactions. Drag one of the <b>Transaction Item</b> <i>prefabs</i> into the Hierarchy, unpack the prefab and rename it. Then make some changes to it (for example height and background color).",
            "Once you've made your changes to the Transaction Item you'll set it as the <b>Transaction Item</b> <i>prefab</i> in the <b>Store View</b> <i>component</i> on your <b>Store</b> <i>prefab</i>.",
            "For the game view to refresh and update to the new <b>Transaction Item</b>, you'll need to either toggle the selected game object in the hierarchy away and back to the Store <i>prefab</i>, or briefly change which tag the prefab shows.",
            "Now that you have your own <b>Transaction Item</b> <i>GameObject</i>, you can create a new purchase button for it. Expand it and delete the existing button. Then create a new UI -> Button, with UI -> Image and UI -> Text <i>GameObjects</i> inside it. Customize the button's dimensions, color, and position however you'd like.",
            "Add the <b>PurchaseButton</b> <i>component</i> to the button and assign the text and image game objects to the PurchaseButton <i>Price Text</i> and <i>Price Image</i> fields. You could also change the No Price String from its default.",
            "Lastly add your new button to the <b>Purchase Button</b> fields in the <b>Transaction Item View</b> <i>component</i> attached to the <b>Transaction Item</b> from a few steps ago. Click on the <b>Store</b> <i>prefab</i> in the hierarchy to see the new button update.",
            "You now have a customized and working Store!"
        };

        /// <summary>
        /// Reference to the panel to display when the wrong database is in use.
        /// </summary>
        public GameObject wrongDatabasePanel;

        /// <summary>
        /// Reference to the panel to display tutorial steps.
        /// </summary>
        public GameObject tutorialPanel;

        /// <summary>
        /// Standard starting point for Unity scripts.
        /// </summary>
        void Awake()
        {
            // The database has been properly setup.
            m_WrongDatabase = !SamplesHelper.VerifyDatabase();
            if (m_WrongDatabase)
            {
                wrongDatabasePanel.SetActive(true);
                tutorialPanel.SetActive(false);
                return;
            }
            
            tutorialPanel.SetActive(true);

            // - Initialize must always be called before working with any game foundation code.
            // - GameFoundation requires an IDataAccessLayer object that will provide and persist
            //   the data required for the various services (Inventory, Wallet, ...).
            // - For this sample we don't need to persist any data so we use the MemoryDataLayer
            //   that will store GameFoundation's data only for the play session.
            GameFoundation.Initialize(new MemoryDataLayer());

            m_NextButton = GameObject.Find("Next Step").GetComponent<Button>();
            m_PreviousButton = GameObject.Find("Previous Step").GetComponent<Button>();
            m_CurrentStepDisplay = GameObject.Find("Step Instructions").GetComponent<Text>();
            m_CurrentStepTitle = GameObject.Find("Step Title").GetComponent<Text>();
            
            m_CurrentStepTitle.text = m_TutorialTitles[m_CurrentStep];
            m_CurrentStepDisplay.text = m_TutorialSteps[m_CurrentStep];
            m_PreviousButton.enabled = false;
        }

        public void NextStep()
        {
            m_CurrentStep++;

            m_NextButton.GetComponentInChildren<Text>().text = m_CurrentStep == m_TutorialSteps.Length - 1 ? "Start Over" : "Next Step";

            if (m_CurrentStep == m_TutorialSteps.Length)
            {
                m_CurrentStep = 0;
                m_PreviousButton.enabled = false;
            }
            else
            {
                m_PreviousButton.enabled = true;
            }

            m_CurrentStepTitle.text = m_TutorialTitles[m_CurrentStep];
            m_CurrentStepDisplay.text = m_TutorialSteps[m_CurrentStep];
        }

        public void PreviousStep()
        {
            m_CurrentStep--;

            m_PreviousButton.enabled = m_CurrentStep != 0;

            m_CurrentStepTitle.text = m_TutorialTitles[m_CurrentStep];
            m_CurrentStepDisplay.text = m_TutorialSteps[m_CurrentStep];
        }
    }
}
