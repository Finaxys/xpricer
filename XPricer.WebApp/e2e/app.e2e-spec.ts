import { XPricer.WebAppPage } from './app.po';

describe('xpricer.web-app App', () => {
  let page: XPricer.WebAppPage;

  beforeEach(() => {
    page = new XPricer.WebAppPage();
  });

  it('should display welcome message', () => {
    page.navigateTo();
    expect(page.getParagraphText()).toEqual('Welcome to app!!');
  });
});
