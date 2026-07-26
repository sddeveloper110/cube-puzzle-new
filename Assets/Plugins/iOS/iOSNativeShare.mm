#import <UIKit/UIKit.h>

extern "C" {
    void _iOS_ShareText(const char* text, const char* url) {
        if (text == NULL && url == NULL) return;
        
        NSMutableArray* itemsToShare = [[NSMutableArray alloc] init];
        
        if (text != NULL && strlen(text) > 0) {
            [itemsToShare addObject:[NSString stringWithUTF8String:text]];
        }
        
        if (url != NULL && strlen(url) > 0) {
            NSURL* nsUrl = [NSURL URLWithString:[NSString stringWithUTF8String:url]];
            if (nsUrl) {
                [itemsToShare addObject:nsUrl];
            }
        }
        
        if (itemsToShare.count == 0) return;
        
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
