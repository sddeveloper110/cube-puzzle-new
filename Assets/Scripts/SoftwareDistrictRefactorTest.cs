using UnityEngine;

namespace SoftwareDistrict.Tests
{
    public class SoftwareDistrictRefactorTest : MonoBehaviour
    {
        public int initialScore = 100;

        private void Start()
        {
            Debug.Log("Refactor test started.");
            MyTestFunctionToRename(initialScore);
        }

        public void MyTestFunctionToRename(int points)
        {
            int calculatedVal = points * 2;
            Debug.Log("Test function value: " + calculatedVal);
        }
    }

    public class SoftwareDistrictRefactorTestHelper
    {
        public void ExecuteTest(SoftwareDistrictRefactorTest target)
        {
            target.MyTestFunctionToRename(50);
        }
    }
}
