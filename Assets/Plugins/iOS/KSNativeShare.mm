// (2026-11-27) iOS 공유 시트 (NativeShareUtil.Share)
#import <UIKit/UIKit.h>

extern UIViewController* UnityGetGLViewController();

extern "C" void _KSShareText(const char* text)
{
    NSString* message = text ? [NSString stringWithUTF8String:text] : @"";
    UIActivityViewController* vc = [[UIActivityViewController alloc] initWithActivityItems:@[message] applicationActivities:nil];
    UIViewController* root = UnityGetGLViewController();
    if (UI_USER_INTERFACE_IDIOM() == UIUserInterfaceIdiomPad)
    {
        vc.popoverPresentationController.sourceView = root.view;
        vc.popoverPresentationController.sourceRect = CGRectMake(root.view.bounds.size.width / 2, root.view.bounds.size.height / 2, 1, 1);
    }
    [root presentViewController:vc animated:YES completion:nil];
}
