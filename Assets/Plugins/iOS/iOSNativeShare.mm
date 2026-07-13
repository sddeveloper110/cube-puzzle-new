#import <UIKit/UIKit.h>

extern "C" {
    void _iOS_ShareText(const char* text) {
        if (text == NULL) return;

        NSString* shareMessage = [NSString stringWithUTF8String:text];
        NSArray* itemsToShare = @[shareMessage];
        
        UIActivityViewController* activityVC = [[UIActivityViewController alloc] initWithActivityItems:itemsToShare applicationActivities:nil];
        
        UIViewController* rootVC = [UIApplication sharedApplication].keyWindow.rootViewController;
        if (rootVC) {
            // On iPad, UIActivityViewController needs to be presented as a popover
            if ([UIDevice currentDevice].userInterfaceIdiom == UIUserInterfaceIdiomPad) {
                activityVC.popoverPresentationController.sourceView = rootVC.view;
                activityVC.popoverPresentationController.sourceRect = CGRectMake(rootVC.view.bounds.size.width / 2, rootVC.view.bounds.size.height / 2, 1, 1);
            }
            [rootVC presentViewController:activityVC animated:YES completion:nil];
        }
    }
}
