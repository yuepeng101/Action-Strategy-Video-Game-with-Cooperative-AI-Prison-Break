using UnityEngine;
using System.Collections;

public class PlayerArcher : MonoBehaviour
{

	public GameObject arrow;
	public Transform arrowSpawner;
	public GameObject animationArrow;

	private bool shooting;
	private bool addArrowForce;
	private GameObject newArrow;
	private float shootingForce;
	private Animator animator;

	private GameObject targetObj;
	PlayerCharacter pc;

	public float launchForce = 10;

	void Start()
	{
		animator = GetComponent<Animator>();
		pc = GetComponent<PlayerCharacter>();
	}

	void Update()
	{
		if (targetObj == null)
		{
			targetObj = pc.FindNearestEnemy();
		}

		float animationTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1;

		//only shoot when animation is almost done (when the character is shooting)
		if (animator.GetBool("Attacking") == true && animationTime >= 0.95f && !shooting)
			StartCoroutine(Shoot());

		animationArrow.SetActive(animationTime > 0.25f && animationTime < 0.95f);
	}



	IEnumerator Shoot()
	{
		//archer is currently shooting
		shooting = true;

		//add a new arrow
		newArrow = Instantiate(arrow, arrowSpawner.position, arrowSpawner.rotation) as GameObject;
		newArrow.GetComponent<Arrow>().arrowOwner = this.gameObject;
		//shoot it using rigidbody addforce
		addArrowForce = true;

		if (targetObj != null)
		{
			shootingForce = Vector3.Distance(transform.position, targetObj.transform.position);

			Vector3 FacingToEnemy = (targetObj.transform.position - transform.position).normalized;
			Quaternion lookWhereYouAreGoing = Quaternion.LookRotation(FacingToEnemy, Vector3.up);
			transform.rotation = Quaternion.RotateTowards(transform.rotation, lookWhereYouAreGoing, 100f);

			Vector3 force = new Vector3(0, shootingForce * 12 + ((targetObj.transform.position.y - transform.position.y) * 45), shootingForce * 55);
			newArrow.GetComponent<Rigidbody>().AddForce(transform.TransformDirection(force));

		}
		else
		{
			newArrow.GetComponent<Rigidbody>().velocity = launchForce * arrowSpawner.forward;
		}

		//wait and set shooting back to false
		yield return new WaitForSeconds(0.5f);
		shooting = false;
	}
}
