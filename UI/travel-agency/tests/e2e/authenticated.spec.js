const { test, expect } = require('@playwright/test');
const api = process.env.E2E_API_URL || 'http://127.0.0.1:5132';
const inbox = process.env.E2E_MAIL_URL || 'http://127.0.0.1:8025';

test('admin email MFA, customer recovery, booking and cancellation work end to end', async ({ page, request }, testInfo) => {
  test.skip(process.env.E2E_FULL_STACK !== 'true' || testInfo.project.name !== 'chromium', 'Requires an isolated SQL database and local Mailpit inbox; run Tools/verify-windows.ps1 or the integration pipeline.');
  test.setTimeout(120000);
  const name = 'e2e' + Date.now();
  const email = name + '@example.test';
  const password = 'BrowserTest123!';
  async function call(path, method='GET', data, token) {
    const response = await request.fetch(api+'/api/v1'+path, {method, data, headers: token ? {Authorization:'Bearer '+token} : {}});
    expect(response.ok(), method+' '+path+' '+await response.text()).toBeTruthy();
    return (await response.json()).data;
  }
  async function getMail(address, pattern) {
    let message;
    await expect.poll(async()=>{
      const response=await request.get(inbox+'/api/v1/messages');
      const body=await response.json();
      message=body.messages.find(m=>m.To.some(to=>to.Address===address)&&pattern.test(m.Subject));
      return Boolean(message);
    },{timeout:20000}).toBe(true);
    return (await (await request.get(inbox+'/api/v1/message/'+message.ID)).json()).Text;
  }

  // Perform privileged login through the real browser and read its code only from email.
  await page.goto('/loginpage', { waitUntil: 'domcontentloaded' });
  await page.getByLabel('Email or username').fill('admin@travelagency.com');
  await page.getByLabel('Password',{exact:true}).fill('Admin123!');
  await page.getByRole('button',{name:'Login',exact:true}).click();
  await expect(page.getByLabel('MFA code')).toBeVisible();
  await expect(page.getByText('Demo code:',{exact:false})).toHaveCount(0);
  const mfa=(await getMail('admin@travelagency.com',/MFA/)).match(/\b\d{6}\b/)[0];
  await page.getByLabel('MFA code').fill(mfa);
  await page.getByRole('button',{name:'Verify MFA'}).click();
  await expect(page).toHaveURL(/dashboard/);
  const admin=await page.evaluate(()=>JSON.parse(localStorage.getItem('travelAgency.auth')));
  expect(admin.role).toBe('admin');
  await page.goto('/admin', { waitUntil: 'domcontentloaded' });
  await expect(page.locator('main h1')).toBeVisible();
  const place=await call('/places','POST',{place_Name:name,place_Description:'Isolated browser test destination'},admin.token);
  const hotel=await call('/hotels','POST',{place_Id:place.id,hotel_Name:name,hotel_Description:'Isolated test hotel',hotel_Stars:4},admin.token);
  const trip=await call('/packages','POST',{place_Id:place.id,hotel_Id:hotel.id,package_Name:name,package_Description:'Isolated browser test package',price_Per_Person:125,start_Date:'2030-01-01',end_Date:'2030-01-10',available_Seats:5},admin.token);
  await call('/auth/register','POST',{u_Name:'Browser',u_Surname:'Tester',u_Email:email,u_Username:name,password,confirmPassword:password});

  await page.evaluate(()=>localStorage.clear());
  await page.goto('/forgot-password', { waitUntil: 'domcontentloaded' });
  await page.getByLabel('Account email').fill(email);
  await page.getByRole('button',{name:'Email reset code'}).click();
  await expect(page.getByText(/If an account exists/)).toBeVisible();
  const reset=(await getMail(email,/Reset your/)).match(/\b[A-F0-9]{64}\b/)[0];
  await expect(page.getByLabel('Reset code',{exact:true})).toHaveValue('');
  await page.getByLabel('Reset code',{exact:true}).fill(reset);
  await page.getByLabel('New password',{exact:true}).fill('ChangedBrowser123!');
  await page.getByLabel('Confirm password',{exact:true}).fill('ChangedBrowser123!');
  await page.getByRole('button',{name:'Reset password',exact:true}).click();
  await expect(page.getByText(/Password reset successfully/)).toBeVisible();
  await page.goto('/loginpage', { waitUntil: 'domcontentloaded' });
  await page.getByLabel('Email or username').fill(email);
  await page.getByLabel('Password',{exact:true}).fill('ChangedBrowser123!');
  await page.getByRole('button',{name:'Login',exact:true}).click();
  await expect(page).toHaveURL(/dashboard/);
  await page.goto('/packages/'+trip.id, { waitUntil: 'domcontentloaded' });
  await page.getByLabel('Travelers').fill('2');
  const created=page.waitForResponse(r=>r.url().endsWith('/api/v1/bookings')&&r.request().method()==='POST');
  await page.getByRole('button',{name:'Book this package'}).click();
  expect((await created).status()).toBe(201);
  await page.goto('/my-bookings', { waitUntil: 'domcontentloaded' });
  const row=page.getByRole('row').filter({hasText:name});
  await expect(row).toContainText('Pending');
  page.once('dialog',dialog=>dialog.accept());
  await row.getByRole('button',{name:'Cancel',exact:true}).click();
  await expect(row).toContainText('Cancelled');
  expect((await call('/packages/'+trip.id)).available_Seats).toBe(5);
  // Records belong only to the disposable test database, destroyed by the harness.
});
